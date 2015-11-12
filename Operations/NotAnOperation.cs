using System.Threading.Tasks;
using Inedo.BuildMaster.Extensibility.Operations;

namespace Inedo.BuildMasterExtensions.Windows.Operations
{
    internal abstract class NotAnOperation : ExecuteOperation
    {
        internal static new Task Complete => ExecuteOperation.Complete;
    }
}
