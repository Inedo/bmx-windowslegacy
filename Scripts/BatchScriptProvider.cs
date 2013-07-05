using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using Inedo.BuildMaster.Extensibility.Actions;
using Inedo.BuildMaster.Extensibility.Providers;
using Inedo.BuildMaster.Extensibility.Providers.ScriptHosts;

namespace Inedo.BuildMasterExtensions.Windows.Scripts
{
    /// <summary>
    /// Provides support for executing Windows batch scripts using CMD.EXE.
    /// </summary>
    [ProviderProperties(
        "Windows Batch Script",
        "Provides support for executing Windows batch scripts using CMD.EXE.")]
    public sealed class BatchScriptProvider : ScriptHostProviderBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BatchScriptProvider"/> class.
        /// </summary>
        public BatchScriptProvider()
        {
        }

        /// <summary>
        /// Gets a value indicating whether the script host accepts arguments.
        /// </summary>
        public override bool SupportsArguments
        {
            get { return true; }
        }
        /// <summary>
        /// Gets a value indicating whether the script host can return a value from a script.
        /// </summary>
        public override bool SupportsReturnValue
        {
            get { return true; }
        }

        /// <summary>
        /// When implemented in a derived class, executes a script file using
        /// a specified execution context.
        /// </summary>
        /// <param name="context">Context in which to execute the script file.</param>
        /// <param name="fileName">Path to the script file to execute.</param>
        /// <param name="arguments">Arguments to pass to the script. This may be null or empty if no arguments are provided.</param>
        /// <returns>
        /// Return value of the script. This may be null if there is no return value.
        /// </returns>
        public override object ExecuteScript(ExecutionContext context, string fileName, ScriptArgument[] arguments)
        {
            string args;
            if (arguments != null && arguments.Length > 0)
            {
                var buffer = new StringBuilder("\"");
                foreach (var argument in arguments)
                {
                    buffer.Append(argument.Value);
                    buffer.Append("\" ");
                }

                args = buffer.ToString(0, buffer.Length - 1);
            }
            else
            {
                args = string.Empty;
            }

            var cmdPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "cmd.exe");
            var process = Process.Start(new ProcessStartInfo
            {
                FileName = cmdPath,
                Arguments = "/C \"" + fileName + "\" " + args,
                UseShellExecute = false
            });

            process.WaitForExit();
            return process.ExitCode;
        }
        /// <summary>
        /// When implemented in a derived class, indicates whether the provider
        /// is installed and available for use in the current execution context.
        /// </summary>
        public override bool IsAvailable()
        {
            return true;
        }
        /// <summary>
        /// When implemented in a derived class, attempts to connect with the
        /// current configuration and, if not successful, throws a
        /// <see cref="ConnectionException"/>.
        /// </summary>
        public override void ValidateConnection()
        {
            if (Environment.OSVersion.Platform != PlatformID.Win32NT)
                throw new ConnectionException("Windows batch scripts can only be executed on a Windows NT based operating system.");
        }
        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return "Provides support for executing Windows batch scripts using CMD.EXE.";
        }
    }
}
