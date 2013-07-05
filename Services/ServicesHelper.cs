using System;
using System.Reflection;
using System.ServiceProcess;
using Inedo.BuildMaster;
using Inedo.BuildMaster.Extensibility.Agents;
using Inedo.Linq;

namespace Inedo.BuildMasterExtensions.Windows.Services
{
    [Serializable]
    internal sealed class ServicesHelper
    {
        private IPersistedObjectExecuter agent;

        /// <summary>
        /// Initializes a new instance of the <see cref="ServicesHelper"/> class.
        /// </summary>
        public ServicesHelper()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ServicesHelper"/> class.
        /// </summary>
        /// <param name="agent">The agent.</param>
        public ServicesHelper(IPersistedObjectExecuter agent)
        {
            this.agent = agent;
        }

        /// <summary>
        /// Determines whether this instance is available.
        /// </summary>
        /// <returns>
        /// 	<c>true</c> if this instance is available; otherwise, <c>false</c>.
        /// </returns>
        public bool IsAvailable()
        {
            try { ValidateServiceAccess(); return true; }
            catch { return false; }
        }

        /// <summary>
        /// Gets the service names.
        /// </summary>
        /// <returns></returns>
        public string[] GetServiceNames()
        {
            if (this.agent == null)
            {
                return ServiceController.GetServices().Select(svc => svc.ServiceName).OrderBy(name => name).ToArray();
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

        private void ValidateServiceAccess()
        {
            if (this.agent == null)
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
            else
            {
                this.agent.ExecuteMethodOnXmlPersistedObject(
                    Util.Persistence.SerializeToPersistedObjectXml(this),
                    MethodBase.GetCurrentMethod().Name,
                    new object[0]
                );
            }
        }
    }
}
