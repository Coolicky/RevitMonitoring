using System.IO;
using System.Reflection;
using Microsoft.Extensions.Configuration;

namespace Monitoring.Revit.Extensions
{
    public static class Configuration
    {
        public static IConfigurationRoot JsonConfiguration()
        {
            var app = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var configurationBuilder = new ConfigurationBuilder()
                .AddJsonFile(Path.Combine(app, "appSettings.json"));
            return configurationBuilder.Build();
        }
    }
}