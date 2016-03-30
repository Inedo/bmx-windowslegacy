using Inedo.BuildMaster.Extensibility.Operations;
using Inedo.BuildMasterExtensions.Windows.Operations.Services;
using Inedo.BuildMasterExtensions.Windows.Services;

namespace Inedo.BuildMasterExtensions.Windows.ActionImporters
{
    internal sealed class StopServiceImporter : IActionOperationConverter<StopServiceAction, StopServiceOperation>
    {
        public ConvertedOperation<StopServiceOperation> ConvertActionToOperation(StopServiceAction action, IActionConverterContext context)
        {
            return new StopServiceOperation
            {
                ServiceName = context.ConvertLegacyExpression(action.ServiceName),
                WaitForStoppedStatus = action.WaitForStop
            };
        }
    }
}
