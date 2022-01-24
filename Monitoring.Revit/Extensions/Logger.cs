using System;
using Autodesk.Revit.UI;
using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Formatting.Json;

namespace Monitoring.Revit.Extensions
{
    public static class Logger
    {
        public static ILogger RegisterLogger(IConfigurationRoot config, UIControlledApplication application)
        {
           return new LoggerConfiguration()
                .MinimumLevel.Information()
                .WriteTo.Async(r => r.Seq(config["Seq"]))
                // .WriteTo.Async(r => r.AzureDocumentDB(config["CosmosDB:Uri"], config["CosmosDB:Key"]))
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
        }
    }
}