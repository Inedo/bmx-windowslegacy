using System;
using System.Linq;

namespace Inedo.BuildMasterExtensions.Windows
{
    /// <summary>
    /// This should be removed in SDK v4.4 when CanPerformTask will officially be part of the SDK.
    /// </summary>
    internal static class Shims
    {
        public static bool CanPerformTask(int task, int? applicationGroupId = null, int? applicationId = null, int? environmentId = null, int? serverId = null)
        {
            var type = Type.GetType("Inedo.BuildMaster.Web.Security.WebUserContext,BuildMaster");
            var canPerformTask = type
                .GetMethods()
                .First(m => m.Name == "CanPerformTask" && m.GetParameters().Length == 5);

            return (bool)canPerformTask.Invoke(null, new object[] { task, applicationGroupId, applicationId, environmentId, serverId });
        }
    }
}
