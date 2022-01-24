using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Autodesk.Revit.DB.Events;
using Autodesk.Revit.UI;
using Microsoft.Extensions.Configuration;
using Revit.DependencyInjection.Unity.Applications;
using Revit.DependencyInjection.Unity.Base;
using Serilog;
using Serilog.Formatting.Json;
using Unity;
using Unity.Lifetime;

namespace Monitoring.Revit
{
    [ContainerProvider("91EA445D-CED3-48AD-BBC8-0CB2844E1A80")]
    public class App : RevitApp
    {
        private static IUnityContainer UnityContainer { get; set; }
        public override Result OnStartup(IUnityContainer container, UIControlledApplication application)
        {
            UnityContainer = container;
            var app = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var configurationBuilder = new ConfigurationBuilder()
                .AddJsonFile(Path.Combine(app, "appSettings.json"));
            var config = configurationBuilder.Build();
            
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .WriteTo.File(new JsonFormatter(",\n"),"C:\\Logs\\log.json")
                .WriteTo.AzureDocumentDB(config["CosmosDB:Uri"], config["CosmosDB:Key"])
                .Enrich.FromLogContext()
                .Enrich.WithProperty("machineName", Environment.MachineName)
                .Enrich.WithProperty("userName", Environment.UserName)
                .Enrich.WithProperty("domain", Environment.UserDomainName)
                .Enrich.WithProperty("operatingSystem", Environment.OSVersion)
                .Enrich.WithProperty("revitVersionName", application.ControlledApplication.VersionName)
                .Enrich.WithProperty("revitVersion", application.ControlledApplication.VersionNumber)
                .Enrich.WithProperty("revitVersion", application.ControlledApplication.VersionBuild)
                .Enrich.WithProperty("revitType", application.ControlledApplication.Product.ToString())
                .Enrich.WithProperty("revitLanguage", application.ControlledApplication.Language.ToString())
                .CreateLogger();
            
            
            
            
            
            
            Log.Information("Revit Started");

            application.ControlledApplication.ApplicationInitialized += Initialized;
            application.ControlledApplication.DocumentOpening += DocOpening;
            application.ControlledApplication.DocumentOpened += DocOpened;
            return Result.Succeeded;
        }
        
        private void DocOpened(object sender, DocumentOpenedEventArgs e)
        {
            
            var timer = UnityContainer.Resolve<ITimer>("DocumentOpening");
            timer.AddArgs("Document", e.Document.PathName);
            timer.Stop();
            Log.Information("Revit Document {Document} Opened", e.Document.PathName);
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
            
            Log.Information("{Document} Opening Started", e.DocumentType);
        }

        private void Initialized(object sender, ApplicationInitializedEventArgs e)
        {
            Log.Information("Revit Application Initialized");
        }
        
        public override Result OnShutdown(IUnityContainer container, UIControlledApplication application)
        {
            Log.CloseAndFlush();
            return Result.Succeeded;
        }
    }
}