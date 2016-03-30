using Inedo.BuildMaster.Extensibility.Operations;
using Inedo.BuildMasterExtensions.Windows.Iis;
using Inedo.BuildMasterExtensions.Windows.Operations.IIS;

namespace Inedo.BuildMasterExtensions.Windows.Legacy.ActionImporters
{
    public sealed class StartAppPoolImporter : IActionOperationConverter<StartupIisAppAction, StartAppPoolOperation>
    {
        public ConvertedOperation<StartAppPoolOperation> ConvertActionToOperation(StartupIisAppAction action, IActionConverterContext context)
        {
            return new StartAppPoolOperation
            {
                AppPool = context.ConvertLegacyExpression(action.AppPool)
            };
        }
    }
}
