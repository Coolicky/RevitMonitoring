using System;
using Autodesk.Revit.UI;
using Microsoft.Extensions.Configuration;
using Monitoring.Revit.Logging;
using Serilog;
using Unity;

namespace Monitoring.Revit.Extensions
{
    public static class ApplicationExtensions
    {
        public static ILogger RegisterLogger(IConfigurationRoot config, UIControlledApplication application)
        {
           return new LoggerConfiguration()
                .MinimumLevel.Information()
                .WriteTo.Async(r => r.Seq(config["Seq:Uri"], apiKey: config["Seq:Key"]))
                .WriteTo.Async(r => r.AzureDocumentDB(config["CosmosDB:Uri"], config["CosmosDB:Key"]))
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
        }
        
        public static IUnityContainer RegisterEventHandler(this IUnityContainer container, IConfigurationRoot config)
        {
            container.RegisterSingleton<Events>();
            var eventConfiguration = new EventConfiguration();
            config.Bind("Events", eventConfiguration);
            container.RegisterInstance(eventConfiguration);
            container.RegisterSingleton<IdleTimer>();
            
            return container;
        }
    }
}