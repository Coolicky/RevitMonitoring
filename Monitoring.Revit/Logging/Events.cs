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
        private readonly EventConfiguration _eventConfig;
        private readonly IdleTimer _idleTimer;

        public Events(IUnityContainer unityContainer, EventConfiguration eventConfig, IdleTimer idleTimer)
        {
            _unityContainer = unityContainer;
            _eventConfig = eventConfig;
            _idleTimer = idleTimer;
        }

        public void SubscribeToEvents(UIControlledApplication application)
        {
            if (_eventConfig.ViewChanged || _eventConfig.TimeSpent)
                application.ViewActivated += DocViewActivated;

            if (_eventConfig.Initialized)
                application.ControlledApplication.ApplicationInitialized += Initialized;
            if (_eventConfig.Opening)
            {
                application.ControlledApplication.DocumentOpening += DocOpening;
                application.ControlledApplication.DocumentOpened += DocOpened;
            }

            if (_eventConfig.Changes)
                application.ControlledApplication.DocumentChanged += DocChanged;
            if (_eventConfig.Synchronizing)
            {
                application.ControlledApplication.DocumentSynchronizingWithCentral += DocSynchronizing;
                application.ControlledApplication.DocumentSynchronizedWithCentral += DocSynchronized;
            }

            if (_eventConfig.Saving)
            {
                application.ControlledApplication.DocumentSaving += DocSaving;
                application.ControlledApplication.DocumentSaved += DocSaved;
            }

            if (_eventConfig.Printing)
                application.ControlledApplication.DocumentPrinted += DocPrinted;
            if (_eventConfig.Exporting)
                application.ControlledApplication.FileExported += FileExported;
            if (_eventConfig.Importing)
                application.ControlledApplication.FileImported += FileImported;
            if (_eventConfig.FamilyLoading)
                application.ControlledApplication.FamilyLoadedIntoDocument += FamilyLoaded;
            
            if (_eventConfig.UiClicks)
                ComponentManager.ItemExecuted += UiButtonClicked;
        }

        public void UnsubscribeToEvents(UIControlledApplication application)
        {
            if (_eventConfig.ViewChanged || _eventConfig.TimeSpent)
                application.ViewActivated -= DocViewActivated;

            if (_eventConfig.Initialized)
                application.ControlledApplication.ApplicationInitialized -= Initialized;
            if (_eventConfig.Opening || _eventConfig.TimeSpent)
            {
                application.ControlledApplication.DocumentOpening -= DocOpening;
                application.ControlledApplication.DocumentOpened -= DocOpened;
            }

            if (_eventConfig.Changes)
                application.ControlledApplication.DocumentChanged -= DocChanged;
            if (_eventConfig.Synchronizing)
            {
                application.ControlledApplication.DocumentSynchronizingWithCentral -= DocSynchronizing;
                application.ControlledApplication.DocumentSynchronizedWithCentral -= DocSynchronized;
            }

            if (_eventConfig.Saving)
            {
                application.ControlledApplication.DocumentSaving -= DocSaving;
                application.ControlledApplication.DocumentSaved -= DocSaved;
            }

            if (_eventConfig.Printing)
                application.ControlledApplication.DocumentPrinted -= DocPrinted;
            if (_eventConfig.Exporting)
                application.ControlledApplication.FileExported -= FileExported;
            if (_eventConfig.Importing)
                application.ControlledApplication.FileImported -= FileImported;
            if (_eventConfig.FamilyLoading)
                application.ControlledApplication.FamilyLoadedIntoDocument -= FamilyLoaded;
            
            if (_eventConfig.UiClicks)
                ComponentManager.ItemExecuted -= UiButtonClicked;
        }

        private void DocViewActivated(object sender, ViewActivatedEventArgs e)
        {            
            if (Log.Logger == null) return;
            if (!e.IsValidObject) return;
            
            if (_eventConfig.ViewChanged)
            {
                var data = new Dictionary<string, object>
                {
                    { "viewName", e.CurrentActiveView.Name },
                    { "documentPath", e.Document.PathName },
                    { "documentTitle", e.Document.Title }
                };
                Log.Information("View Activated: {Data}", data);
            }

            if (_eventConfig.TimeSpent)
            {
                var documentChanged = e.CurrentActiveView.Document.PathName != e.PreviousActiveView.Document.PathName;
                if (documentChanged)
                {
                    _idleTimer.ChangeDocument();
                }
            }
        }

        private void FamilyLoaded(object sender, FamilyLoadedIntoDocumentEventArgs e)
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

        private void FileImported(object sender, FileImportedEventArgs e)
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

        private void FileExported(object sender, FileExportedEventArgs e)
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

        private void DocPrinted(object sender, DocumentPrintedEventArgs e)
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

        private void Initialized(object sender, ApplicationInitializedEventArgs e)
        {
            var type = sender.GetType();
            if (Log.Logger == null) return;
            Log.Information("Revit Application Initialized");
        }

        private void DocOpening(object sender, DocumentOpeningEventArgs e)
        {
            if (Log.Logger == null) return;
            if (_unityContainer == null) return;
            if (!e.IsValidObject) return;
            if (e.DocumentType != DocumentType.Project) return;

            if (_eventConfig.Opening)
            {
                var args = new Dictionary<string, object>
                {
                    { "DocumentPath", e.PathName },
                    { "DocumentType", e.DocumentType }
                };

                var timer = new Timer("Opening Document", args);
                timer.Start();
                _unityContainer.RegisterInstance(typeof(ITimer), "DocumentOpening", timer, new SingletonLifetimeManager());    
            }
        }

        private void DocOpened(object sender, DocumentOpenedEventArgs e)
        {
            if (Log.Logger == null) return;
            if (_unityContainer == null) return;
            if (!e.IsValidObject) return;

            if (_eventConfig.Opening)
            {
                var timer = _unityContainer.Resolve<ITimer>("DocumentOpening");
                if (timer == null) return;
                if (!timer.Stopwatch.IsRunning) return;

                timer.AddArgs("documentPath", e.Document.PathName);
                timer.AddArgs("documentTitle", e.Document.Title);
                timer.Stop();
                Log.Information("Revit Document {Document} Opened", e.Document.PathName);
            }

            if (_eventConfig.TimeSpent)
            {
                _idleTimer.StartIdleTimer();
            }
        }

        private void DocSaving(object sender, DocumentSavingEventArgs e)
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

        private void DocSaved(object sender, DocumentSavedEventArgs e)
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
        
        private void DocSynchronizing(object sender, DocumentSynchronizingWithCentralEventArgs e)
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

        private void DocSynchronized(object sender, DocumentSynchronizedWithCentralEventArgs e)
        {
            if (Log.Logger == null) return;
            if (!e.IsValidObject) return;

            var timer = _unityContainer.Resolve<ITimer>("DocumentSynchronizing");
            timer.AddArgs("documentPath", e.Document.PathName);
            timer.AddArgs("documentTitle", e.Document.Title);
            timer.Stop();
            Log.Information("Revit Document {Document} Synchronized", e.Document.PathName);
        }
        
        private void DocChanged(object sender, DocumentChangedEventArgs e)
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
        
        private void UiButtonClicked(object sender, RibbonItemExecutedEventArgs e)
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