using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Events;
using Autodesk.Revit.UI;
using Monitoring.Revit.Commands.Availability;
using Monitoring.Revit.Commands.HelloWorld;
using Monitoring.Revit.Commands.SampleInjection;
using Monitoring.Revit.Commands.SampleWindows;
using Revit.DependencyInjection.Unity.Applications;
using Revit.DependencyInjection.Unity.Async;
using Revit.DependencyInjection.Unity.Base;
using Revit.DependencyInjection.Unity.UI;
using Unity;
using Unity.Lifetime;

namespace Monitoring.Revit
{
    [ContainerProvider("91EA445D-CED3-48AD-BBC8-0CB2844E1A80")]
    public class App : RevitApp
    {
        public override void OnCreateRibbon(IRibbonManager ribbonManager)
        {
            // var br = ribbonManager.GetLineBreak();
            //
            // var sampleTab = ribbonManager.CreateTab("Sample Tab");
            // var samplePanel = ribbonManager.CreatePanel(sampleTab.GetTabName(), "Sample Panel");
            //
            // samplePanel.AddPushButton<HelloWorldCommand, AvailableAlways>($"Hello{br}World", "hello");
            // samplePanel.AddPushButton<SampleInjectionCommand, AvailableOnProject>($"Get{br}Selection", "selection");
            // samplePanel.AddPushButton<SampleWindowCommand, AvailableOnProject>($"Show{br}Window", "window");
        }
        
        public static void AddTimeStamp(string text)
        {            
            using (var streamWriter = new StreamWriter("C:\\Logs\\log.txt",true))
            {
                var builder = new StringBuilder();
                builder.Append($" MachineName:{Environment.MachineName}");
                builder.Append($" UserName:{Environment.UserName}");
                builder.Append($" Domain:{Environment.UserDomainName}");
                builder.Append($" OperatingSystem:{Environment.OSVersion}");
                streamWriter.WriteLine($"{DateTime.Now:O} -> {text}{builder}");
            }
        }

        private static IUnityContainer UnityContainer { get; set; }
        public override Result OnStartup(IUnityContainer container, UIControlledApplication application)
        {
            // container.AddRevitAsync(GetAsyncSettings);
            // container.RegisterSampleServices();
            // container.RegisterViews();
            // container.RegisterViewModels();
            
            
            
            UnityContainer = container;
            AddTimeStamp("Startup");
            
            application.ControlledApplication.ApplicationInitialized += Initialized;
            application.ControlledApplication.DocumentOpening += DocOpening;
            application.ControlledApplication.DocumentOpened += DocOpened;
            return Result.Succeeded;
        }
        
        private void DocOpened(object sender, DocumentOpenedEventArgs e)
        {
            
            var timer = UnityContainer.Resolve<ITimer>("DocumentOpening");
            timer.Stop();
            AddTimeStamp("Opened");
        }

        private void DocOpening(object sender, DocumentOpeningEventArgs e)
        {
            var args = new Dictionary<string, object>()
            {
                { "DocumentPath", e.PathName },
                { "DocumentType", e.DocumentType }
            };
            var timer = new Timer(args);
            timer.Start();
            UnityContainer.RegisterInstance(typeof(ITimer), "DocumentOpening", timer , new SingletonLifetimeManager());
            
            AddTimeStamp("Opening");
        }

        private void Initialized(object sender, ApplicationInitializedEventArgs e)
        {
            AddTimeStamp("Initialized");
        }
        
        public override Result OnShutdown(IUnityContainer container, UIControlledApplication application)
        {
            return Result.Succeeded;
        }

        private void GetAsyncSettings(RevitAsyncSettings settings)
        {
            settings.Name = "Revit DI Samples";
            settings.IsJournalable = true;
        }
    }

    public interface ITimer
    {
        Stopwatch Stopwatch { get; set; }
        void Start();
        void Stop();
        void Log();
    }

    public class Timer : ITimer
    {
        private readonly Dictionary<string, object> _args;

        public Timer(Dictionary<string, object> args)
        {
            _args = args;
            Stopwatch = new Stopwatch();
        }
        public Stopwatch Stopwatch { get; set; }
        public void Start()
        {
            Stopwatch.Reset();
            Stopwatch.Start();
        }

        public void Stop()
        {
            Stopwatch.Stop();
            Log();
        }

        public void Log()
        {
            var builder = new StringBuilder();
            builder.Append(string.Join(";", _args.Select(r => $" {r.Key}-{r.Value}")));
            builder.Append($"Timer: {Stopwatch.ElapsedMilliseconds} seconds");
            App.AddTimeStamp(builder.ToString());
            
        }
    }
}