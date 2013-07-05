using System;
using System.Collections.Generic;

namespace Inedo.BuildMasterExtensions.Windows.Iis
{
    /// <summary>
    /// Provides methods for interfacing with IIS.
    /// </summary>
    internal abstract partial class IISUtil
    {
        private static readonly object instanceLock = new object();
        private static WeakReference instance;

        /// <summary>
        /// Initializes a new instance of the <see cref="IISUtil"/> class.
        /// </summary>
        protected IISUtil()
        {
        }

        /// <summary>
        /// Gets the IIS management instance.
        /// </summary>
        public static IISUtil Instance
        {
            get
            {
                lock (instanceLock)
                {
                    if (instance == null)
                    {
                        var inst = CreateInstance();
                        instance = new WeakReference(inst);
                        return inst;
                    }

                    var current = instance.Target as IISUtil;
                    if (current == null)
                    {
                        current = CreateInstance();
                        instance = new WeakReference(current);
                    }

                    return current;
                }
            }
        }

        /// <summary>
        /// Returns a collection of the names of all AppPools on the local system.
        /// </summary>
        /// <returns>Names of all of the AppPools on the local system.</returns>
        public abstract IEnumerable<string> GetAppPoolNames();
        /// <summary>
        /// Starts an AppPool on the local system.
        /// </summary>
        /// <param name="name">Name of the AppPool to start.</param>
        public abstract void StartAppPool(string name);
        /// <summary>
        /// Stops an AppPool on the local system.
        /// </summary>
        /// <param name="name">Name of the AppPool to stop.</param>
        public abstract void StopAppPool(string name);

        /// <summary>
        /// Returns a new instances of the newest supported IIS management interface.
        /// </summary>
        /// <returns>Instance of the newest supported IIS management interface.</returns>
        private static IISUtil CreateInstance()
        {
            try
            {
                return new IIS7Util();
            }
            catch
            {
            }

            try
            {
                return new IIS6Util();
            }
            catch
            {
            }

            throw new InvalidOperationException("IIS 6 or newer management interfaces are not present on the system.");
        }
    }
}
