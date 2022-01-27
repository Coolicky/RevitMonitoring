using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using WixSharp;
using WixSharp.CommonTasks;
using WixSharp.UI;
using File = System.IO.File;
using Project = WixSharp.Project;

namespace Monitoring.Setup
{
    internal static class Program
    {
        private static readonly DateTime ProjectStartedDate = new DateTime(year: 2022, month: 1, day: 26);
#if DEBUG
        const string Configuration = "Debug";
#else
        const string Configuration = "Release";
#endif
        static readonly string BuildDirectory = $"\\bin\\x64\\{Configuration}\\net48";
        private const string PluginProjectName = "Monitoring.Revit";
        private const string SetupProjectName = "Monitoring.Setup";
        private const string WpfProjectName = "Monitoring.Setup.Wpf";
        const string ProjectName = "Revit Monitoring";
        const string OutputName = "Monitoring.Setup";
        const string OutputDir = "output";

        public static void Main(string[] args)
        {
            Console.WriteLine($"Using {Configuration} configuration.");
            var pluginDirectory = BuildPluginProject();

            var project = CreateProject(pluginDirectory);

            Console.WriteLine($"Building Installer!");
            project.BuildMsi();

            CopyMsiFile(project);
            BuildWpfProject();
        }

        private static Project CreateProject(string pluginDirectory)
        {
            var fileName = new StringBuilder().Append(OutputName).Append("-").Append(GetVersion());

            Console.WriteLine($"Creating new MSI Project {fileName}");
            var project = new Project
            {
                Name = ProjectName,
                OutDir = OutputDir,
                Platform = Platform.x64,
                UI = WUI.WixUI_Minimal,
                Version = GetVersion(),
                OutFileName = fileName.ToString(),
                InstallScope = InstallScope.perUser,
                MajorUpgrade = MajorUpgrade.Default,
                GUID = new Guid("BF597DA2-5D8F-4293-A3B7-87480891144B"),
                ControlPanelInfo =
                {
                    Manufacturer = Environment.UserName,
                    ProductIcon = @"coolicky.ico"
                },
                LaunchConditions = new List<LaunchCondition>
                {
                    new LaunchCondition("CUSTOM_UI=\"true\" OR REMOVE=\"ALL\"", "Please run setup.exe instead."),
                }
            };

            project.AddDir(new Dir(GetInstallationDirectory(), new WixSharp.File(
                $@"{Directory.GetCurrentDirectory()}\PackageContents.xml")));
            project.AddDir(
                new Dir(Path.Combine(GetInstallationDirectory(), "Contents"), Files.FromBuildDir(pluginDirectory)));
            return project;
        }

        private static void CopyMsiFile(Project project)
        {
            var directory = Directory.GetCurrentDirectory();
            var msiFile = Path.Combine(directory, project.OutDir, $"{project.OutFileName}.msi");
            var projectDirectory = directory.Replace(BuildDirectory, "");
            var msiFolder = Path.Combine(projectDirectory, project.OutDir);
            var msiCopy = Path.Combine(msiFolder, $"{OutputName}.msi");
            Directory.CreateDirectory(Path.GetDirectoryName(msiCopy) ?? throw new InvalidOperationException());
            Directory.GetFiles(msiFolder).ForEach(File.Delete);
            Console.WriteLine($"Copying MSI from {msiFile} to \n{msiCopy}");
            File.Copy(msiFile, msiCopy);

            var parser = new MsiParser(msiCopy);
            var x =parser.GetProductName();
            var y = x;
        }

        private static string GetInstallationDirectory()
        {
            var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            return Path.Combine(appData, "Autodesk", "ApplicationPlugins", "Revit.Monitoring");
        }

        private static string BuildPluginProject()
        {
            var directory = Directory.GetCurrentDirectory();
            var pluginDirectory = directory.Replace(SetupProjectName, PluginProjectName);
            var pluginProject =
                pluginDirectory.Replace(BuildDirectory, $"{PluginProjectName}.csproj");

            Console.WriteLine($"Plugin Project: {pluginProject}");

            var process = new Process();
            process.StartInfo = new ProcessStartInfo(@"dotnet")
            {
                Arguments =
                    $"build {pluginProject} " +
                    $"--configuration {Configuration} " +
                    $"/p:Platform=x64 " +
                    $"-p:Version={GetVersion()} " +
                    $"-p:ProductVersion={GetVersion()}",
                UseShellExecute = false,
                RedirectStandardOutput = true
            };
            Console.WriteLine($"Starting Plugin Build with args: {process.StartInfo.Arguments}");
            process.Start();
            Console.WriteLine(process.StandardOutput.ReadToEnd());
            process.WaitForExit(60 * 1000);
            Console.WriteLine("Plugin Built!");
            return pluginDirectory;
        }

        private static void BuildWpfProject()
        {
            var directory = Directory.GetCurrentDirectory();
            var wpfDirectory = directory.Replace(SetupProjectName, WpfProjectName);
            var wpfProjectDirectory =
                wpfDirectory.Replace(BuildDirectory, "");
            var wpfProject = Path.Combine(wpfProjectDirectory, $"{WpfProjectName}.csproj");

            Console.WriteLine($"Installer Wpf Project: {wpfProject}");

            var process = new Process();
            process.StartInfo = new ProcessStartInfo(@"dotnet")
            {
                Arguments =
                    $"build {wpfProject} " +
                    $"--configuration {Configuration} " +
                    $"/p:Platform=x64 " +
                    $"-p:Version={GetVersion()} " +
                    $"-p:ProductVersion={GetVersion()} ",
                UseShellExecute = false,
                RedirectStandardOutput = true
            };
            Console.WriteLine($"Starting Installer Build with args: {process.StartInfo.Arguments}");
            process.Start();
            Console.WriteLine(process.StandardOutput.ReadToEnd());
            process.WaitForExit(60 * 1000);
            Console.WriteLine("Installer Built!");
        }

        private static Version GetVersion()
        {
            const int majorVersion = 0;
            const int minorVersion = 7;
            var daysSinceProjectStarted = (int)((DateTime.UtcNow - ProjectStartedDate).TotalDays);
            var minutesSinceMidnight = (int)DateTime.UtcNow.TimeOfDay.TotalMinutes;
            var version = $"{majorVersion}.{minorVersion}.{daysSinceProjectStarted}.{minutesSinceMidnight}";
            return new Version(version);
        }
    }
}