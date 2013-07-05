using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Reflection;

namespace Inedo.BuildMasterExtensions.Windows.SqlServer
{
    internal static class ProviderScripts
    {
        static readonly string Initialize = "Inedo.BuildMasterExtensions.Windows.SqlServer.ProviderScripts.Initialize.sql";
        static readonly string ReInitFrom1 = "Inedo.BuildMasterExtensions.Windows.SqlServer.ProviderScripts.ReInitFrom1.sql";
        static readonly string ReInitFrom2 = "Inedo.BuildMasterExtensions.Windows.SqlServer.ProviderScripts.ReInitFrom2.sql";

        static readonly Assembly asm = typeof(ProviderScripts).Assembly;
        static string Get(string resourceName)
        {
            return new StreamReader(asm.GetManifestResourceStream(resourceName)).ReadToEnd();
        }

        public static string GetInitialize()
        {
            return Get(Initialize);
        }

        public static string GetReInitFrom1()
        {
            return Get(ReInitFrom1);
        }

        public static string GetReInitFrom2()
        {
            return Get(ReInitFrom2);
        }

    }
}
