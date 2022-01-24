using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Monitoring.Revit.Commands.SampleWindows.Views;
using Revit.DependencyInjection.Unity.Commands;
using Unity;

namespace Monitoring.Revit.Commands.SampleWindows
{
    [Transaction(TransactionMode.Manual)]
    public class SampleWindowCommand : RevitAppCommand<App>
    {
        public override Result Execute(IUnityContainer container, ExternalCommandData commandData, ref string message,
            ElementSet elements)
        {
            var window = container.Resolve<SampleWindow>();
            window.Show();

            return Result.Succeeded;
        }
    }
}