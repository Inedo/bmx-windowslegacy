using Inedo.BuildMaster;
using Inedo.BuildMaster.Extensibility.Configurers.Extension;
using Inedo.BuildMaster.Web;

[assembly: ExtensionConfigurer(typeof(Inedo.BuildMasterExtensions.Windows.WindowsExtensionConfigurer))]

namespace Inedo.BuildMasterExtensions.Windows
{
    [CustomEditor(typeof(WindowsExtensionConfigurerEditor))]
    public sealed class WindowsExtensionConfigurer : ExtensionConfigurerBase
    {
        [Persistent]
        public bool OverridePowerShellDefaults { get; set; }

        public override string ToString()
        {
            return string.Empty;
        }
    }
}
