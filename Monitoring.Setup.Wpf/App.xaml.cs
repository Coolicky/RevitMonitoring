using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace Monitoring.Setup.Wpf
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        static Window _splashscreen;
        static public string MsiFile { get; set; }

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            _splashscreen = new SplashScreen();
            _splashscreen.Show();
            App.DoEvents();

            byte[] msiData = Monitoring.Setup.Wpf.Properties.Resources.Msi_File;
            MsiFile = Path.Combine(Path.GetTempPath(), "MyProduct.msi");

            if (!File.Exists(MsiFile) || new FileInfo(MsiFile).Length != msiData.Length)
                File.WriteAllBytes(MsiFile, msiData);
        }

        public static void HideSplashScreen()
        {
            _splashscreen.Close();
        }

        public static void DoEvents()
        {
            Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new Action(delegate { })); 
        }
    }
}