using Monitoring.Revit.Commands.SampleWindows.ViewModels;
using Monitoring.Revit.Commands.SampleWindows.Views;
using Monitoring.Revit.Interfaces;
using Unity;

namespace Monitoring.Revit
{
    public static class SamplePipeline
    {
        public static IUnityContainer RegisterSampleServices(this IUnityContainer container)
        {
            container.RegisterType<ISampleSelector, SampleSelector>();
            return container;
        }

        public static IUnityContainer RegisterViews(this IUnityContainer container)
        {
            container.RegisterType<SampleWindow>();
            return container;
        }

        public static IUnityContainer RegisterViewModels(this IUnityContainer container)
        {
            container.RegisterType<SampleWindowViewModel>();
            return container;
        }
    }
}