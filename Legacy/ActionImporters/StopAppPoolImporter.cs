using Inedo.BuildMaster.Extensibility.Operations;
using Inedo.BuildMasterExtensions.Windows.Iis;
using Inedo.BuildMasterExtensions.Windows.Operations.IIS;

namespace Inedo.BuildMasterExtensions.Windows.Legacy.ActionImporters
{
    class StopAppPoolImporter : IActionOperationConverter<ShutdownIisAppAction, StopAppPoolOperation>
    {
        public ConvertedOperation<StopAppPoolOperation> ConvertActionToOperation(ShutdownIisAppAction action, IActionConverterContext context)
        {
            return new StopAppPoolOperation
            {
                AppPool = context.ConvertLegacyExpression(action.AppPool)
            };
        }
    }
}
