using System.Collections.Generic;
using Autodesk.Internal.Windows;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Events;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Events;
using Autodesk.Windows;
using Serilog;
using Unity;
using Unity.Lifetime;

namespace Monitoring.Revit.Logging
{
    public class Events
    {
        private readonly IUnityContainer _unityContainer;
        private readonly IdleTimer _idleTimer;

        public Events(IUnityContainer unityContainer, IdleTimer idleTimer)
        {
            _unityContainer = unityContainer;
            _idleTimer = idleTimer;
        }

        public void SubscribeToEvents(UIControlledApplication application)
        {
            //TODO: Configure this through appSettings.json
            application.ViewActivated += DocViewActivated;

            application.ControlledApplication.ApplicationInitialized += Initialized;
            application.ControlledApplication.DocumentOpening += DocOpening;
            application.ControlledApplication.DocumentOpened += DocOpened;
            application.ControlledApplication.DocumentChanged += DocChanged;
            application.ControlledApplication.DocumentSynchronizingWithCentral += DocSynchronizing;
            application.ControlledApplication.DocumentSynchronizedWithCentral += DocSynchronized;
            application.ControlledApplication.DocumentSaving += DocSaving;
            application.ControlledApplication.DocumentSaved += DocSaved;
            application.ControlledApplication.DocumentPrinted += DocPrinted;
            application.ControlledApplication.FileExported += FileExported;
            application.ControlledApplication.FileImported += FileImported;
            application.ControlledApplication.FamilyLoadedIntoDocument += FamilyLoaded;

            ComponentManager.ItemExecuted += UiButtonClicked;
        }
        
        public void UnsubscribeToEvents(UIControlledApplication application)
        {
            //TODO: Configure this through appSettings.json
            application.ViewActivated -= DocViewActivated;

            application.ControlledApplication.ApplicationInitialized -= Initialized;
            application.ControlledApplication.DocumentOpening -= DocOpening;
            application.ControlledApplication.DocumentOpened -= DocOpened;
            application.ControlledApplication.DocumentSynchronizingWithCentral -= DocSynchronizing;
            application.ControlledApplication.DocumentSynchronizedWithCentral -= DocSynchronized;
            application.ControlledApplication.DocumentSaving -= DocSaving;
            application.ControlledApplication.DocumentSaved -= DocSaved;
            application.ControlledApplication.DocumentPrinted -= DocPrinted;
            application.ControlledApplication.FileExported -= FileExported;
            application.ControlledApplication.FileImported -= FileImported;
            application.ControlledApplication.FamilyLoadedIntoDocument -= FamilyLoaded;

            ComponentManager.ItemExecuted -= UiButtonClicked;
        }

        public void DocViewActivated(object sender, ViewActivatedEventArgs e)
        {
            if (Log.Logger == null) return;
            if (!e.IsValidObject) return;

            var data = new Dictionary<string, object>
            {
                { "viewName", e.CurrentActiveView.Name },
                { "documentPath", e.Document.PathName },
                { "documentTitle", e.Document.Title }
            };
            Log.Information("View Activated: {Data}", data);

            var documentChanged = e.CurrentActiveView.Document.PathName != e.PreviousActiveView.Document.PathName;
            if (documentChanged)
            {
                _idleTimer.ChangeDocument();
            }
        }

        public void FamilyLoaded(object sender, FamilyLoadedIntoDocumentEventArgs e)
        {
            if (Log.Logger == null) return;
            if (!e.IsValidObject) return;

            var data = new Dictionary<string, object>
            {
                { "familyName", e.FamilyName },
                { "familyPath", e.FamilyPath },
                { "documentPath", e.Document.PathName },
                { "documentTitle", e.Document.Title },
                { "override", e.OriginalFamilyId == null && e.OriginalFamilyId == ElementId.InvalidElementId }
            };
            Log.Information("Family Loaded: {Data}", data);
        }

        public void FileImported(object sender, FileImportedEventArgs e)
        {
            if (Log.Logger == null) return;
            if (!e.IsValidObject) return;

            var data = new Dictionary<string, object>
            {
                { "fileFormat", e.Format },
                { "path", e.Path },
                { "documentPath", e.Document.PathName },
                { "documentTitle", e.Document.Title },
            };
            Log.Information("File Imported: {Data}", data);
        }

        public void FileExported(object sender, FileExportedEventArgs e)
        {
            if (Log.Logger == null) return;
            if (!e.IsValidObject) return;

            var data = new Dictionary<string, object>
            {
                { "fileFormat", e.Format },
                { "path", e.Path },
                { "documentPath", e.Document.PathName },
                { "documentTitle", e.Document.Title },
            };
            Log.Information("File Exported: {Data}", data);
        }

        public void DocPrinted(object sender, DocumentPrintedEventArgs e)
        {
            if (Log.Logger == null) return;
            if (!e.IsValidObject) return;

            var printedViewIds = e.GetPrintedViewElementIds();

            foreach (var viewId in printedViewIds)
            {
                var data = new Dictionary<string, object>
                {
                    { "view", e.Document.GetElement(viewId) is View view ? view.Name : "Could not be determined" },
                    { "documentPath", e.Document.PathName },
                    { "documentTitle", e.Document.Title },
                };
                Log.Information("Printed Document: {Data}", data);
            }
        }

        public void Initialized(object sender, ApplicationInitializedEventArgs e)
        {
            if (Log.Logger == null) return;
            Log.Information("Revit Application Initialized");
        }

        public void DocOpening(object sender, DocumentOpeningEventArgs e)
        {
            if (Log.Logger == null) return;
            if (_unityContainer == null) return;
            if (!e.IsValidObject) return;
            if (e.DocumentType != DocumentType.Project) return;

            var args = new Dictionary<string, object>
            {
                { "DocumentPath", e.PathName },
                { "DocumentType", e.DocumentType }
            };

            var timer = new Timer("Opening Document", args);
            timer.Start();
            _unityContainer.RegisterInstance(typeof(ITimer), "DocumentOpening", timer, new SingletonLifetimeManager());
        }

        public void DocOpened(object sender, DocumentOpenedEventArgs e)
        {
            if (Log.Logger == null) return;
            if (_unityContainer == null) return;
            if (!e.IsValidObject) return;

            var timer = _unityContainer.Resolve<ITimer>("DocumentOpening");
            if (timer == null) return;
            if (!timer.Stopwatch.IsRunning) return;
            
            timer.AddArgs("documentPath", e.Document.PathName);
            timer.AddArgs("documentTitle", e.Document.Title);
            timer.Stop();
            Log.Information("Revit Document {Document} Opened", e.Document.PathName);
            _idleTimer.StartIdleTimer();
        }

        public void DocSaving(object sender, DocumentSavingEventArgs e)
        {
            if (Log.Logger == null) return;
            if (_unityContainer == null) return;
            if (!e.IsValidObject) return;

            var args = new Dictionary<string, object>
            {
                { "documentPath", e.Document.PathName },
                { "documentTitle", e.Document.Title }
            };

            var timer = new Timer("Saving Document", args);
            timer.Start();
            _unityContainer.RegisterInstance(typeof(ITimer), "DocumentSaving", timer,
                new SingletonLifetimeManager());
        }

        public void DocSaved(object sender, DocumentSavedEventArgs e)
        {
            if (Log.Logger == null) return;
            if (_unityContainer == null) return;
            if (!e.IsValidObject) return;

            var timer = _unityContainer.Resolve<ITimer>("DocumentSaving");
            timer.AddArgs("documentPath", e.Document.PathName);
            timer.AddArgs("documentTitle", e.Document.Title);
            timer.Stop();
            Log.Information("Revit Document {Document} Saved", e.Document.PathName);
        }


        public void DocSynchronizing(object sender, DocumentSynchronizingWithCentralEventArgs e)
        {
            if (Log.Logger == null) return;
            if (_unityContainer == null) return;
            if (!e.IsValidObject) return;

            var args = new Dictionary<string, object>
            {
                { "documentPath", e.Document.PathName },
                { "documentTitle", e.Document.Title },
                { "Comments", e.Comments }
            };

            var timer = new Timer("Synchronizing Document", args);
            timer.Start();
            _unityContainer.RegisterInstance(typeof(ITimer), "DocumentSynchronizing", timer,
                new SingletonLifetimeManager());
        }

        public void DocSynchronized(object sender, DocumentSynchronizedWithCentralEventArgs e)
        {
            if (Log.Logger == null) return;
            if (!e.IsValidObject) return;

            var timer = _unityContainer.Resolve<ITimer>("DocumentSynchronizing");
            timer.AddArgs("documentPath", e.Document.PathName);
            timer.AddArgs("documentTitle", e.Document.Title);
            timer.Stop();
            Log.Information("Revit Document {Document} Synchronized", e.Document.PathName);
        }

        public void DocChanged(object sender, DocumentChangedEventArgs e)
        {
            if (Log.Logger == null) return;
            if (!e.IsValidObject) return;

            var data = new Dictionary<string, string>
            {
                { "documentPath", e.GetDocument().PathName },
                { "documentTitle", e.GetDocument().Title },
                { "deletedElements", e.GetDeletedElementIds().Count.ToString() },
                { "modifiedElements", e.GetModifiedElementIds().Count.ToString() },
                { "addedElements", e.GetAddedElementIds().Count.ToString() }
            };
            Log.Information("Document Modified: {Data}", data);
        }

        //TODO: Probably Logging this is Overkill too
        public void UiButtonClicked(object sender, RibbonItemExecutedEventArgs e)
        {
            if (Log.Logger == null) return;
            if (e.Item == null) return;

            var data = new Dictionary<string, string>
            {
                { "buttonId", e.Item.Text.Replace("\r\n", " ") },
                { "buttonName", e.Item.Id }
            };
            Log.Information("Button Clicked: {Data}", data);
        }
    }
}