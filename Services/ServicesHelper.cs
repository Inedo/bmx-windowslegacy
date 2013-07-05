using System;
using System.Linq;
using System.ServiceProcess;

namespace Inedo.BuildMasterExtensions.Windows.Services
{
    internal static class ServicesHelper
    {
        public static bool IsAvailable()
        {
            try { ValidateServiceAccess(); return true; }
            catch { return false; }
        }

        public static string[] GetServiceNames()
        {
            return ServiceController.GetServices().Select(svc => svc.ServiceName).OrderBy(name => name).ToArray();
        }

        private static void ValidateServiceAccess()
        {
            try
            {
                ServiceController.GetServices();
            }
            catch (Exception e)
            {
                throw new InvalidOperationException("Services are not available", e);
            }
        }
    }
}
