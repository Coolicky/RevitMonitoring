using Autodesk.Revit.UI;
using Autodesk.Windows;
using Monitoring.Revit.Extensions;
using Monitoring.Revit.Logging;
using Revit.DependencyInjection.Unity.Applications;
using Revit.DependencyInjection.Unity.Base;
using Serilog;
using Unity;

namespace Monitoring.Revit
{
    [ContainerProvider("91EA445D-CED3-48AD-BBC8-0CB2844E1A80")]
    public class App : RevitApp
    {
        public static IUnityContainer UnityContainer { get; set; }

        public override Result OnStartup(IUnityContainer container, UIControlledApplication application)
        {
            var config = Configuration.JsonConfiguration();
            Log.Logger = Logger.RegisterLogger(config, application);

            Log.Information("Revit Started");

            application.ViewActivated += Events.DocViewActivated;
            
            application.ControlledApplication.ApplicationInitialized += Events.Initialized;
            application.ControlledApplication.DocumentOpening += Events.DocOpening;
            application.ControlledApplication.DocumentOpened += Events.DocOpened;
            application.ControlledApplication.DocumentChanged += Events.DocChanged;
            application.ControlledApplication.DocumentSynchronizingWithCentral += Events.DocSynchronizing;
            application.ControlledApplication.DocumentSynchronizedWithCentral += Events.DocSynchronized;
            application.ControlledApplication.DocumentSaving += Events.DocSaving;
            application.ControlledApplication.DocumentSaved += Events.DocSaved;
            application.ControlledApplication.DocumentPrinted += Events.DocPrinted;
            application.ControlledApplication.FileExported += Events.FileExported;
            application.ControlledApplication.FileImported += Events.FileImported;
            application.ControlledApplication.FamilyLoadedIntoDocument += Events.FamilyLoaded;

            ComponentManager.ItemExecuted += Events.UiButtonClicked;

            return Result.Succeeded;
        }


        public override Result OnShutdown(IUnityContainer container, UIControlledApplication application)
        {
            application.ViewActivated -= Events.DocViewActivated;
            
            application.ControlledApplication.ApplicationInitialized -= Events.Initialized;
            application.ControlledApplication.DocumentOpening -= Events.DocOpening;
            application.ControlledApplication.DocumentOpened -= Events.DocOpened;            
            application.ControlledApplication.DocumentSynchronizingWithCentral -= Events.DocSynchronizing;
            application.ControlledApplication.DocumentSynchronizedWithCentral -= Events.DocSynchronized;
            application.ControlledApplication.DocumentSaving -= Events.DocSaving;
            application.ControlledApplication.DocumentSaved -= Events.DocSaved;
            application.ControlledApplication.DocumentPrinted -= Events.DocPrinted;
            application.ControlledApplication.FileExported -= Events.FileExported;
            application.ControlledApplication.FileImported -= Events.FileImported;
            application.ControlledApplication.FamilyLoadedIntoDocument -= Events.FamilyLoaded;

            ComponentManager.ItemExecuted -= Events.UiButtonClicked;

            Log.CloseAndFlush();
            return Result.Succeeded;
        }
    }
}