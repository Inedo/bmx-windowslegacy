using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.ServiceProcess;
using System.Text;

namespace Inedo.BuildMasterExtensions.Windows.Services
{
    internal static class ServicesHelper
    {
        public static bool IsAvailable()
        {
            try { ValidateServiceAccess(); return true; }
            catch { return false; }
        }

        public static KeyValuePair<string, string>[] GetServices()
        {
            return EnumerateServices()
                .OrderBy(s => s.Value)
                .ThenBy(s => s.Key)
                .ToArray();
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

        private static List<KeyValuePair<string, string>> EnumerateServices()
        {
            var ptr = Marshal.AllocCoTaskMem(64 * 1024);
            try
            {
                var scHandle = NativeMethods.OpenSCManager(null, null, 1 /*SC_MANAGER_CONNECT*/ | 4 /*SC_MANAGER_ENUMERATE_SERVICE*/);
                try
                {
                    var services = new List<KeyValuePair<string, string>>();

                    uint bytesNeeded;
                    uint servicesReturned;
                    uint resumeHandle = 0;

                    NativeMethods.EnumServicesStatus(scHandle, 0x30 /*SERVICE_WIN32*/, 3 /*SERVICE_STATE_ALL*/, ptr, 64 * 1024, out bytesNeeded, out servicesReturned, ref resumeHandle);

                    unsafe
                    {
                        var data = (ENUM_SERVICE_STATUS*)ptr.ToPointer();
                        for (uint i = 0; i < servicesReturned; i++)
                        {
                            var name = Marshal.PtrToStringUni(data[i].lpServiceName);
                            var displayName = Marshal.PtrToStringUni(data[i].lpDisplayName);
                            services.Add(new KeyValuePair<string, string>(name, displayName));
                        }
                    }

                    return services;
                }
                finally
                {
                    if (scHandle != IntPtr.Zero)
                        NativeMethods.CloseServiceHandle(scHandle);
                }
            }
            finally
            {
                if (ptr != IntPtr.Zero)
                    Marshal.FreeCoTaskMem(ptr);
            }
        }

        private static class NativeMethods
        {
            [DllImport("advapi32.dll", EntryPoint = "OpenSCManagerW", CallingConvention = CallingConvention.Winapi, CharSet = CharSet.Unicode, SetLastError = true)]
            public static extern IntPtr OpenSCManager(
                [MarshalAs(UnmanagedType.LPWStr)] string lpMachineName,
                [MarshalAs(UnmanagedType.LPWStr)] string lpDatabaseName,
                uint dwDesiredAccess
            );

            [DllImport("advapi32.dll", EntryPoint = "GetServiceDisplayNameW", CallingConvention = CallingConvention.Winapi, CharSet = CharSet.Unicode, SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool GetServiceDisplayName(
                IntPtr hSCManager,
                [MarshalAs(UnmanagedType.LPWStr)] string lpServiceName,
                [MarshalAs(UnmanagedType.LPWStr)] StringBuilder lpDisplayName,
                ref uint lpcchBuffer
            );

            [DllImport("advapi32.dll", EntryPoint = "EnumServicesStatusW", CallingConvention = CallingConvention.Winapi, CharSet = CharSet.Unicode, SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool EnumServicesStatus(
                IntPtr hSCManager,
                uint dwServiceType,
                uint dwServiceState,
                IntPtr lpServices,
                uint cbBufSize,
                out uint pcbBytesNeeded,
                out uint lpServicesReturned,
                ref uint lpResumeHandle
            );

            [DllImport("advapi32.dll", CallingConvention = CallingConvention.Winapi, SetLastError = true)]
            public static extern bool CloseServiceHandle(
                IntPtr hSCObject
            );
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct ENUM_SERVICE_STATUS
        {
            public IntPtr lpServiceName;
            public IntPtr lpDisplayName;
            public SERVICE_STATUS ServiceStatus;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct SERVICE_STATUS
        {
            public uint dwServiceType;
            public uint dwCurrentState;
            public uint dwControlsAccepted;
            public uint dwWin32ExitCode;
            public uint dwServiceSpecificExitCode;
            public uint dwCheckPoint;
            public uint dwWaitHint;
        }
    }
}
