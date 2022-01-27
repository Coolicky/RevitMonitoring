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
            if (Log.Logger == null) return;
            if (application?.ControlledApplication == null) return;

            try
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
            catch (Exception e)
            {
                Log.Error(e, "Error");
            }
        }

        public void UnsubscribeToEvents(UIControlledApplication application)
        {
            if (application?.ControlledApplication == null) return;
            try
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
            catch (Exception e)
            {
                Log.Error(e, "Error");
            }
        }

        private void DocViewActivated(object sender, ViewActivatedEventArgs e)
        {
            if (_eventConfig.ViewChanged)
            {
                try
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
                catch (Exception exception)
                {
                    Log.Error(exception, "Error");
                }
            }

            if (_eventConfig.TimeSpent)
            {
                try
                {
                    var documentChanged =
                        e.CurrentActiveView?.Document?.PathName != e.PreviousActiveView?.Document?.PathName;
                    if (documentChanged)
                    {
                        _idleTimer.ChangeDocument();
                    }
                }
                catch (Exception exception)
                {
                    Log.Error(exception, "Error");
                }
            }
        }

        private void FamilyLoaded(object sender, FamilyLoadedIntoDocumentEventArgs e)
        {
            try
            {
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
            catch (Exception exception)
            {
                Log.Error(exception, "Error");
            }
        }

        private void FileImported(object sender, FileImportedEventArgs e)
        {
            try
            {
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
            catch (Exception exception)
            {
                Log.Error(exception, "Error");
            }
        }

        private void FileExported(object sender, FileExportedEventArgs e)
        {
            try
            {
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
            catch (Exception exception)
            {
                Log.Error(exception, "Error");
            }
        }

        private void DocPrinted(object sender, DocumentPrintedEventArgs e)
        {
            try
            {
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
                        { isSheet ? "sheet" : "view", viewName },
                        { "documentPath", documentPath },
                        { "documentTitle", e.Document.Title },
                    };
                    Log.Information("Printed Document: {Data}", data);
                }
            }
            catch (Exception exception)
            {
                Log.Error(exception, "Error");
            }
        }

        private void Initialized(object sender, ApplicationInitializedEventArgs e)
        {
            try
            {
                Log.Information("Revit Application Initialized");
            }
            catch (Exception exception)
            {
                Log.Error(exception, "Error");
            }
        }

        private void DocOpening(object sender, DocumentOpeningEventArgs e)
        {
            try
            {
                if (!_eventConfig.Opening) return;

                _openingTimer.AddArgs("DocumentPath", e.PathName);
                _openingTimer.AddArgs("DocumentType", e.DocumentType);

                _openingTimer.Start();
            }
            catch (Exception exception)
            {
                Log.Error(exception, "Error");
            }
        }

        private void DocOpened(object sender, DocumentOpenedEventArgs e)
        {
            if (_eventConfig.Opening)
            {
                try
                {
                    if (_openingTimer == null) return;
                    if (!_openingTimer.Stopwatch.IsRunning) return;

                    var documentPath = e.Document.PathName;
                    _openingTimer.AddArgs("documentPath", documentPath);
                    _openingTimer.AddArgs("documentTitle", e.Document.Title);
                    _openingTimer.AddArgs("documentSize", GetDocumentSize(documentPath));
                    _openingTimer.Stop();
                }
                catch (Exception exception)
                {
                    Log.Error(exception, "Error");
                }
            }

            if (_eventConfig.TimeSpent)
            {
                try
                {
                    _idleTimer.StartIdleTimer();
                }
                catch (Exception exception)
                {
                    Log.Error(exception, "Error");
                }
            }
        }

        private void DocSaving(object sender, DocumentSavingEventArgs e)
        {
            try
            {
                _savingTimer.Start();
            }
            catch (Exception exception)
            {
                Log.Error(exception, "Error");
            }
        }

        private void DocSaved(object sender, DocumentSavedEventArgs e)
        {
            try
            {
                var documentPath = e.Document.PathName;
                _savingTimer.AddArgs("documentPath", documentPath);
                _savingTimer.AddArgs("documentTitle", e.Document.Title);
                _savingTimer.AddArgs("documentSize", GetDocumentSize(documentPath));
                _savingTimer.Stop();
            }
            catch (Exception exception)
            {
                Log.Error(exception, "Error");
            }
        }

        private void DocSavingAs(object sender, DocumentSavingAsEventArgs e)
        {
            _savingAsTimer.Start();
        }

        private void DocSavedAs(object sender, DocumentSavedAsEventArgs e)
        {
            try
            {
                var documentPath = e.Document.PathName;
                _savingAsTimer.AddArgs("documentPath", documentPath);
                _savingAsTimer.AddArgs("documentTitle", e.Document.Title);
                _savingAsTimer.AddArgs("documentSize", GetDocumentSize(documentPath));
                _savingAsTimer.AddArgs("masterFile", e.IsSavingAsMasterFile);
                _savingAsTimer.AddArgs("originalDocumentPath", e.OriginalPath);
                _savingTimer.Stop();
            }
            catch (Exception exception)
            {
                Log.Error(exception, "Error");
            }
        }


        private void DocSynchronizing(object sender, DocumentSynchronizingWithCentralEventArgs e)
        {
            try
            {
                _synchronizingTimer.AddArgs("comments", e.Comments);
                _synchronizingTimer.Start();
            }
            catch (Exception exception)
            {
                Log.Error(exception, "Error");
            }
        }

        private void DocSynchronized(object sender, DocumentSynchronizedWithCentralEventArgs e)
        {
            try
            {
                var documentPath = e.Document.PathName;
                _synchronizingTimer.AddArgs("documentPath", documentPath);
                _synchronizingTimer.AddArgs("documentTitle", e.Document.Title);
                _synchronizingTimer.AddArgs("documentSize", GetDocumentSize(documentPath));
                _synchronizingTimer.Stop();
            }
            catch (Exception exception)
            {
                Log.Error(exception, "Error");
            }
        }

        private void DocChanged(object sender, DocumentChangedEventArgs e)
        {
            try
            {
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
            catch (Exception exception)
            {
                Log.Error(exception, "Error");
            }
        }

        private void UiButtonClicked(object sender, RibbonItemExecutedEventArgs e)
        {
            try
            {
                var data = new Dictionary<string, string>
                {
                    { "buttonId", e.Item.Text.Replace("\r\n", " ") },
                    { "buttonName", e.Item.Id }
                };
                Log.Information("Button Clicked: {Data}", data);
            }
            catch (Exception exception)
            {
                Log.Error(exception, "Error");
            }
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