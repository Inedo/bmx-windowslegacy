using System.Linq;

namespace Inedo.BuildMasterExtensions.Windows.Iis
{
    /// <summary>
    /// Proxy for the IISUtil class.
    /// </summary>
    internal static class ProxiedUtil
    {
        /// <summary>
        /// Returns a list of the names of the AppPools on the server.
        /// </summary>
        /// <returns>List of AppPool names on the server.</returns>
        public static string[] GetAppPoolNames()
        {
            return IISUtil.Instance.GetAppPoolNames().ToArray();
        }
    }
}
