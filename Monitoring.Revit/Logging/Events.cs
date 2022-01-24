using System.Collections.Generic;
using Autodesk.Internal.Windows;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Events;
using Autodesk.Revit.UI.Events;
using Serilog;
using Unity;
using Unity.Lifetime;

namespace Monitoring.Revit.Logging
{
    public static class Events
    {
        public static void DocViewActivated(object sender, ViewActivatedEventArgs e)
        {
            if (Log.Logger == null) return;
            if (!e.IsValidObject) return;

            var data = new Dictionary<string, object>
            {
                { "viewName", e.CurrentActiveView.Name },
                { "document", e.Document.PathName }
            };
            Log.Information("View Activated: {Data}", data);
        }

        public static void FamilyLoaded(object sender, FamilyLoadedIntoDocumentEventArgs e)
        {
            if (Log.Logger == null) return;
            if (!e.IsValidObject) return;

            var data = new Dictionary<string, object>
            {
                { "familyName", e.FamilyName },
                { "familyPath", e.FamilyPath },
                { "override", e.OriginalFamilyId == null && e.OriginalFamilyId == ElementId.InvalidElementId }
            };
            Log.Information("Family Loaded: {Data}", data);
        }

        public static void FileImported(object sender, FileImportedEventArgs e)
        {
            if (Log.Logger == null) return;
            if (!e.IsValidObject) return;

            var data = new Dictionary<string, object>
            {
                { "fileFormat", e.Format },
                { "path", e.Path },
                { "document", e.Document.PathName }
            };
            Log.Information("File Imported: {Data}", data);
        }

        public static void FileExported(object sender, FileExportedEventArgs e)
        {
            if (Log.Logger == null) return;
            if (!e.IsValidObject) return;

            var data = new Dictionary<string, object>
            {
                { "fileFormat", e.Format },
                { "path", e.Path },
                { "document", e.Document.PathName }
            };
            Log.Information("File Exported: {Data}", data);
        }

        public static void DocPrinted(object sender, DocumentPrintedEventArgs e)
        {
            if (Log.Logger == null) return;
            if (!e.IsValidObject) return;

            var printedViewIds = e.GetPrintedViewElementIds();

            foreach (var viewId in printedViewIds)
            {
                var data = new Dictionary<string, object>
                {
                    { "view", e.Document.GetElement(viewId) is View view ? view.Name : "Could not be determined" },
                    { "document", e.Document.PathName }
                };
                Log.Information("Printed Document: {Data}", data);
            }
        }

        public static void Initialized(object sender, ApplicationInitializedEventArgs e)
        {
            if (Log.Logger == null) return;
            Log.Information("Revit Application Initialized");
        }

        public static void DocOpening(object sender, DocumentOpeningEventArgs e)
        {
            if (Log.Logger == null) return;
            if (App.UnityContainer == null) return;
            if (!e.IsValidObject) return;
            if (e.DocumentType != DocumentType.Project) return;

            var args = new Dictionary<string, object>
            {
                { "DocumentPath", e.PathName },
                { "DocumentType", e.DocumentType }
            };

            var timer = new Timer("Opening Document", args);
            timer.Start();
            App.UnityContainer.RegisterInstance(typeof(ITimer), "DocumentOpening", timer, new SingletonLifetimeManager());
        }

        public static void DocOpened(object sender, DocumentOpenedEventArgs e)
        {
            if (Log.Logger == null) return;
            if (App.UnityContainer == null) return;
            if (!e.IsValidObject) return;

            var timer = App.UnityContainer.Resolve<ITimer>("DocumentOpening");
            if (timer == null) return;
            if (!timer.Stopwatch.IsRunning) return;

            timer.AddArgs("Document", e.Document.PathName);
            timer.Stop();
            Log.Information("Revit Document {Document} Opened", e.Document.PathName);
        }

        public static void DocSaving(object sender, DocumentSavingEventArgs e)
        {
            if (Log.Logger == null) return;
            if (App.UnityContainer == null) return;
            if (!e.IsValidObject) return;

            var args = new Dictionary<string, object>
            {
                { "DocumentPath", e.Document.PathName }
            };

            var timer = new Timer("Saving Document", args);
            timer.Start();
            App.UnityContainer.RegisterInstance(typeof(ITimer), "DocumentSaving", timer,
                new SingletonLifetimeManager());
        }

        public static void DocSaved(object sender, DocumentSavedEventArgs e)
        {
            if (Log.Logger == null) return;
            if (App.UnityContainer == null) return;
            if (!e.IsValidObject) return;

            var timer = App.UnityContainer.Resolve<ITimer>("DocumentSaving");
            timer.Stop();
            Log.Information("Revit Document {Document} Saved", e.Document.PathName);
        }


        public static void DocSynchronizing(object sender, DocumentSynchronizingWithCentralEventArgs e)
        {
            if (Log.Logger == null) return;
            if (App.UnityContainer == null) return;
            if (!e.IsValidObject) return;

            var args = new Dictionary<string, object>
            {
                { "DocumentPath", e.Location },
                { "Comments", e.Comments }
            };

            var timer = new Timer("Synchronizing Document", args);
            timer.Start();
            App.UnityContainer.RegisterInstance(typeof(ITimer), "DocumentSynchronizing", timer,
                new SingletonLifetimeManager());
        }

        public static void DocSynchronized(object sender, DocumentSynchronizedWithCentralEventArgs e)
        {
            if (Log.Logger == null) return;
            if (!e.IsValidObject) return;

            var timer = App.UnityContainer.Resolve<ITimer>("DocumentSynchronizing");
            timer.Stop();
            Log.Information("Revit Document {Document} Synchronized", e.Document.PathName);
        }

        public static void DocChanged(object sender, DocumentChangedEventArgs e)
        {
            if (Log.Logger == null) return;
            if (!e.IsValidObject) return;

            var data = new Dictionary<string, string>
            {
                { "document", e.GetDocument().PathName },
                { "deletedElements", e.GetDeletedElementIds().Count.ToString() },
                { "modifiedElements", e.GetModifiedElementIds().Count.ToString() },
                { "addedElements", e.GetAddedElementIds().Count.ToString() }
            };
            Log.Information("Document Modified: {Data}", data);
        }

        public static void UiButtonClicked(object sender, RibbonItemExecutedEventArgs e)
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