using System;
using System.Collections.Generic;
using Microsoft.Web.Administration;

namespace Inedo.BuildMasterExtensions.Windows.Iis
{
    internal partial class IISUtil
    {
        /// <summary>
        /// Provides methods for interfacing with IIS 7.
        /// </summary>
        private sealed class IIS7Util : IISUtil
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="IIS7Util"/> class.
            /// </summary>
            public IIS7Util()
            {
                ValidateIis7Management();
            }

            /// <summary>
            /// Returns a collection of the names of all AppPools on the local system.
            /// </summary>
            /// <returns>Names of all of the AppPools on the local system.</returns>
            public override IEnumerable<string> GetAppPoolNames()
            {
                using (var manager = new ServerManager())
                {
                    foreach (var pool in manager.ApplicationPools)
                        yield return pool.Name;
                }
            }
            /// <summary>
            /// Starts an AppPool on the local system.
            /// </summary>
            /// <param name="name">Name of the AppPool to start.</param>
            public override void StartAppPool(string name)
            {
                using (var manager = new ServerManager())
                {
                    var pool = manager.ApplicationPools[name];
                    if (pool == null)
                        throw new InvalidOperationException("Application pool not found.");

                    pool.Start();
                }
            }
            /// <summary>
            /// Stops an AppPool on the local system.
            /// </summary>
            /// <param name="name">Name of the AppPool to stop.</param>
            public override void StopAppPool(string name)
            {
                using (var manager = new ServerManager())
                {
                    var pool = manager.ApplicationPools[name];
                    if (pool == null)
                        throw new InvalidOperationException("Application pool not found.");

                    pool.Stop();
                }
            }

            /// <summary>
            /// Verifies that the required IIS6 interfaces are present.
            /// </summary>
            private void ValidateIis7Management()
            {
                using (var manager = new ServerManager())
                {
                }
            }
        }
    }
}
