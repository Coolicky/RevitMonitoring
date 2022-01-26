using System;
using System.Collections.Generic;
using System.IO;
using Autodesk.Internal.Windows;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Events;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Events;
using Autodesk.Windows;
using Serilog;

namespace Monitoring.Revit.Logging
{
    public class Events
    {
        private readonly EventTimer _openingTimer;
        private readonly EventTimer _savingTimer;
        private readonly EventTimer _savingAsTimer;
        private readonly EventTimer _synchronizingTimer;
        private readonly EventConfiguration _eventConfig;
        private readonly IdleTimer _idleTimer;

        public Events(EventConfiguration eventConfig, IdleTimer idleTimer)
        {
            _eventConfig = eventConfig;
            _idleTimer = idleTimer;
            _openingTimer = new EventTimer("Opening Document");
            _savingTimer = new EventTimer("Saving Document");
            _savingAsTimer = new EventTimer("SavingAs Document");
            _synchronizingTimer = new EventTimer("Synchronizing Document");
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

                application.ControlledApplication.DocumentSavingAs += DocSavingAs;
                application.ControlledApplication.DocumentSavedAs += DocSavedAs;
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
                var documentPath = e.Document.PathName;
                var data = new Dictionary<string, object>
                {
                    { "viewName", e.CurrentActiveView.Name },
                    { "documentPath", documentPath },
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

            var documentPath = e.Document.PathName;
            var data = new Dictionary<string, object>
            {
                { "familyName", e.FamilyName },
                { "familyPath", e.FamilyPath },
                { "documentPath", documentPath },
                { "documentTitle", e.Document.Title },
                { "override", e.OriginalFamilyId == null && e.OriginalFamilyId == ElementId.InvalidElementId }
            };
            Log.Information("Family Loaded: {Data}", data);
        }

        private void FileImported(object sender, FileImportedEventArgs e)
        {
            if (Log.Logger == null) return;
            if (!e.IsValidObject) return;

            var documentPath = e.Document.PathName;
            var data = new Dictionary<string, object>
            {
                { "fileFormat", e.Format },
                { "path", e.Path },
                { "documentPath", documentPath },
                { "documentTitle", e.Document.Title },
            };
            Log.Information("File Imported: {Data}", data);
        }

        private void FileExported(object sender, FileExportedEventArgs e)
        {
            if (Log.Logger == null) return;
            if (!e.IsValidObject) return;

            var documentPath = e.Document.PathName;
            var data = new Dictionary<string, object>
            {
                { "fileFormat", e.Format },
                { "path", e.Path },
                { "documentPath", documentPath },
                { "documentTitle", e.Document.Title },
            };
            Log.Information("File Exported: {Data}", data);
        }

        private void DocPrinted(object sender, DocumentPrintedEventArgs e)
        {
            if (Log.Logger == null) return;
            if (!e.IsValidObject) return;

            var printedViewIds = e.GetPrintedViewElementIds();

            var documentPath = e.Document.PathName;
            foreach (var viewId in printedViewIds)
            {
                var view = e.Document.GetElement(viewId);
                var isSheet = view is ViewSheet;
                var viewName = view.Name;
                if (isSheet)
                    viewName = $"{((ViewSheet)view).SheetNumber}-{viewName}";
                
                var data = new Dictionary<string, object>
                {
                    { isSheet ? "sheet" : "view",  viewName },
                    { "documentPath", documentPath },
                    { "documentTitle", e.Document.Title },
                };
                Log.Information("Printed Document: {Data}", data);
            }
        }

        private void Initialized(object sender, ApplicationInitializedEventArgs e)
        {
            if (Log.Logger == null) return;
            Log.Information("Revit Application Initialized");
        }

        private void DocOpening(object sender, DocumentOpeningEventArgs e)
        {
            if (!_eventConfig.Opening) return;
            if (Log.Logger == null) return;
            if (!e.IsValidObject) return;
            if (e.DocumentType != DocumentType.Project) return;

            _openingTimer.AddArgs("DocumentPath", e.PathName);
            _openingTimer.AddArgs("DocumentType", e.DocumentType);

            _openingTimer.Start();
        }

        private void DocOpened(object sender, DocumentOpenedEventArgs e)
        {
            if (Log.Logger == null) return;
            if (!e.IsValidObject) return;

            if (_eventConfig.Opening)
            {
                if (_openingTimer == null) return;
                if (!_openingTimer.Stopwatch.IsRunning) return;

                var documentPath = e.Document.PathName;
                _openingTimer.AddArgs("documentPath", documentPath);
                _openingTimer.AddArgs("documentTitle", e.Document.Title);
                _openingTimer.AddArgs("documentSize", GetDocumentSize(documentPath));
                _openingTimer.Stop();
            }

            if (_eventConfig.TimeSpent)
            {
                _idleTimer.StartIdleTimer();
            }
        }

        private void DocSaving(object sender, DocumentSavingEventArgs e)
        {
            if (Log.Logger == null) return;
            if (!e.IsValidObject) return;

            _savingTimer.Start();
        }

        private void DocSaved(object sender, DocumentSavedEventArgs e)
        {
            if (Log.Logger == null) return;
            if (!e.IsValidObject) return;

            var documentPath = e.Document.PathName;
            _savingTimer.AddArgs("documentPath", documentPath);
            _savingTimer.AddArgs("documentTitle", e.Document.Title);
            _savingTimer.AddArgs("documentSize", GetDocumentSize(documentPath));
            _savingTimer.Stop();
        }

        private void DocSavingAs(object sender, DocumentSavingAsEventArgs e)
        {
            if (Log.Logger == null) return;
            if (!e.IsValidObject) return;

            _savingAsTimer.Start();
        }

        private void DocSavedAs(object sender, DocumentSavedAsEventArgs e)
        {
            if (Log.Logger == null) return;
            if (!e.IsValidObject) return;

            var documentPath = e.Document.PathName;
            _savingAsTimer.AddArgs("documentPath", documentPath);
            _savingAsTimer.AddArgs("documentTitle", e.Document.Title);
            _savingAsTimer.AddArgs("documentSize", GetDocumentSize(documentPath));
            _savingAsTimer.AddArgs("masterFile", e.IsSavingAsMasterFile);
            _savingAsTimer.AddArgs("originalDocumentPath", e.OriginalPath);
            _savingTimer.Stop();
        }


        private void DocSynchronizing(object sender, DocumentSynchronizingWithCentralEventArgs e)
        {
            if (Log.Logger == null) return;
            if (!e.IsValidObject) return;

            _synchronizingTimer.AddArgs("comments", e.Comments);
            _synchronizingTimer.Start();
        }

        private void DocSynchronized(object sender, DocumentSynchronizedWithCentralEventArgs e)
        {
            if (Log.Logger == null) return;
            if (!e.IsValidObject) return;

            var documentPath = e.Document.PathName;
            _synchronizingTimer.AddArgs("documentPath", documentPath);
            _synchronizingTimer.AddArgs("documentTitle", e.Document.Title);
            _synchronizingTimer.AddArgs("documentSize", GetDocumentSize(documentPath));
            _synchronizingTimer.Stop();
        }

        private void DocChanged(object sender, DocumentChangedEventArgs e)
        {
            if (Log.Logger == null) return;
            if (!e.IsValidObject) return;

            var documentPath = e.GetDocument().PathName;
            var data = new Dictionary<string, string>
            {
                { "documentPath", documentPath },
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

        private static string GetDocumentSize(string path)
        {
            try
            {
                var fileInfo = new FileInfo(path);
                return $"{fileInfo.Length / 1024 / 1024} MB";
            }
            catch (Exception)
            {
                return "N/A";
            }
        }
    }
}