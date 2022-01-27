using System;
using System.IO;
using System.Windows;
using System.Windows.Threading;

namespace Monitoring.Setup.Wpf
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static string MsiFile { get; set; }

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            DoEvents();

            byte[] msiData = Monitoring.Setup.Wpf.Properties.Resources.Msi_File;
            MsiFile = Path.Combine(Path.GetTempPath(), "MyProduct.msi");

            if (!File.Exists(MsiFile) || new FileInfo(MsiFile).Length != msiData.Length)
                File.WriteAllBytes(MsiFile, msiData);
        }

        private static void DoEvents()
        {
            Current.Dispatcher.Invoke(DispatcherPriority.Background, new Action(delegate { })); 
        }
    }
}