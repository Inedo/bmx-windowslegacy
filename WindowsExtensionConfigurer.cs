using Inedo.BuildMaster.Extensibility.Configurers.Extension;
using Inedo.Serialization;
using Inedo.Web;

[assembly: ExtensionConfigurer(typeof(Inedo.BuildMasterExtensions.Windows.WindowsExtensionConfigurer))]

namespace Inedo.BuildMasterExtensions.Windows
{
    [CustomEditor(typeof(WindowsExtensionConfigurerEditor))]
    [PersistFrom("Inedo.BuildMasterExtensions.Windows.WindowsExtensionConfigurer,Windows")]
    public sealed class WindowsExtensionConfigurer : ExtensionConfigurerBase
    {
        [Persistent]
        public bool OverridePowerShellDefaults { get; set; }
    }
}
