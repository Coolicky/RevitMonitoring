using System;
using System.Reflection;
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
        public static IntPtr Handle { get; set; }

        public override Result OnStartup(IUnityContainer container, UIControlledApplication application)
        {
            var config = Configuration.JsonConfiguration();
            Log.Logger = Logger.RegisterLogger(config, application);
            
            var fi = application.GetType().GetField("m_uiapplication", BindingFlags.NonPublic | BindingFlags.Instance);
            if (fi != null)
            {
                var uiApp = (UIApplication)fi.GetValue(application);
                container.RegisterInstance(uiApp);
            }
            
            
            Log.Information("Revit Started");            
            
            container.RegisterSingleton<Events>();
            container.RegisterSingleton<IdleTimer>();
            container.RegisterInstance(application.MainWindowHandle);
            
            var events = container.Resolve<Events>();
            events.SubscribeToEvents(application);

            return Result.Succeeded;
        }

        public override Result OnShutdown(IUnityContainer container, UIControlledApplication application)
        {
            var events = container.Resolve<Events>();
            events.UnsubscribeToEvents(application);

            var idleTimer = container.Resolve<IdleTimer>();
            idleTimer.Dispose();

            Log.CloseAndFlush();
            return Result.Succeeded;
        }
    }
}