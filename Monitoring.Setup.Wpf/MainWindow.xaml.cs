using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace Monitoring.Setup.Wpf
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MyProductSetup Setup { get; set; }
        
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            App.HideSplashScreen();

            Setup = new MyProductSetup(App.MsiFile);
            Setup.InUiThread = this.InUiThread;
            DataContext = Setup;
        }

        public void InUiThread(Action action)
        {
            if (this.Dispatcher.CheckAccess())
                action();
            else
                Dispatcher.BeginInvoke(DispatcherPriority.Normal, action);
        }

        private void Window_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void Minimize_Click(object sender, RoutedEventArgs e)
        {
            WindowState = System.Windows.WindowState.Minimized;
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Install_Click(object sender, RoutedEventArgs e)
        {
            Setup.StartInstall();
        }

        private void Repair_Click(object sender, RoutedEventArgs e)
        {
            Setup.StartRepair();
        }

        private void Uninstall_Click(object sender, RoutedEventArgs e)
        {
            Setup.StartUninstall();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            Setup.CancelRequested = true;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = Setup.IsRunning;
        }

        private void ShowLog_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Process.Start(Setup.LogFile);
        }
        private void Border_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.ChangedButton == System.Windows.Input.MouseButton.Left)
            {
                DragMove();
            }
        }
    }
}