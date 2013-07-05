using System;
using System.Reflection;
using Inedo.BuildMaster;
using Inedo.BuildMaster.Extensibility.Agents;
using Inedo.Linq;

namespace Inedo.BuildMasterExtensions.Windows.Iis
{
    /// <summary>
    /// Proxy for the IISUtil class.
    /// </summary>
    [Serializable]
    internal sealed class ProxiedUtil
    {
        private IPersistedObjectExecuter agent;

        /// <summary>
        /// Initializes a new instance of the <see cref="ProxiedUtil"/> class.
        /// </summary>
        public ProxiedUtil()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ProxiedUtil"/> class.
        /// </summary>
        /// <param name="agent">The agent.</param>
        public ProxiedUtil(IPersistedObjectExecuter agent)
        {
            this.agent = agent;
        }
        

        /// <summary>
        /// Returns a list of the names of the AppPools on the server.
        /// </summary>
        /// <returns>List of AppPool names on the server.</returns>
        public string[] GetAppPoolNames()
        {
            if (this.agent == null)
            {
                return IISUtil.Instance.GetAppPoolNames().ToArray();
            }
            else
            {
                return (string[])this.agent.ExecuteMethodOnXmlPersistedObject(
                    Util.Persistence.SerializeToPersistedObjectXml(this),
                    MethodBase.GetCurrentMethod().Name,
                    new object[0]
                );
            }
        }
    }
}
