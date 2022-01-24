using System.Windows;
using Monitoring.Revit.Commands.SampleWindows.ViewModels;

namespace Monitoring.Revit.Commands.SampleWindows.Views
{
    public partial class SampleWindow : Window
    {
        public SampleWindowViewModel ViewModel { get; }

        public SampleWindow(SampleWindowViewModel viewModel)
        {
            ViewModel = viewModel;
            InitializeComponent();
        }
    }
}