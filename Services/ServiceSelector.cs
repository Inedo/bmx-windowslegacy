using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using Inedo.BuildMaster;
using Inedo.BuildMaster.Data;
using Inedo.BuildMaster.Extensibility.Agents;
using Inedo.BuildMaster.Web.Security;
using Inedo.Web.ClientResources;
using Inedo.Web.Controls;
using Inedo.Web.Handlers;

namespace Inedo.BuildMasterExtensions.Windows.Services
{
    internal sealed class ServiceSelector : HiddenField
    {
        protected override void OnPreRender(EventArgs e)
        {
            this.IncludeClientResourceInPage(
                new JavascriptResource
                {
                    ResourcePath = "/extension-resources/Windows/Services/ServiceSelector.js?" + typeof(ServiceSelector).Assembly.GetName().Version,
                    Dependencies = { InedoLibCR.select2.select2_js },
                    CompatibleVersions = { InedoLibCR.Versions.jq171 }
                }
            );

            base.OnPreRender(e);
        }
        protected override void Render(HtmlTextWriter writer)
        {
            base.Render(writer);

            writer.Write("<script type=\"text/javascript\">");
            writer.Write("BmServiceSelector(");
            InedoLib.Util.JavaScript.WriteJson(
                writer,
                new
                {
                    inputSelector = "#" + this.ClientID,
                    getServicesUrl = DynamicHttpHandling.GetJavascriptDataUrl<int, string, object>(AjaxGetServices)
                }
            );
            writer.Write(");");
            writer.Write("</script>");
        }

        [AjaxMethod]
        public static object AjaxGetServices(int serverId, string term = null)
        {
            try
            {
                if (serverId <= 0)
                    return new object[0];

                if (!WebUserContext.CanPerformTask(SecuredTask.Environments_ViewServer, serverId: serverId))
                    throw new SecurityException();

                var serverInfo = StoredProcs.Environments_GetServer(serverId, 0).Execute().Servers.FirstOrDefault();
                if (serverInfo == null)
                    return new object[0];

                if (serverInfo.ServerType_Code != Domains.ServerTypeCodes.Server)
                {
                    var groupServer = StoredProcs.Environments_GetServersInGroup(serverId, Domains.YN.No)
                        .Execute()
                        .FirstOrDefault(s => WebUserContext.CanPerformTask(SecuredTask.Environments_ViewServer, serverId: s.Server_Id));

                    if (groupServer == null)
                        return new object[0];

                    serverId = groupServer.Server_Id;
                }

                using (var proxy = Util.Proxy.CreateAgentProxy(serverId))
                {
                    var remote = proxy.TryGetService<IRemoteMethodExecuter>();
                    IEnumerable<KeyValuePair<string, string>> serviceNames = (KeyValuePair<string, string>[])remote.InvokeMethod(new Func<KeyValuePair<string, string>[]>(ServicesHelper.GetServices).Method, null, null);

                    if (!string.IsNullOrWhiteSpace(term))
                    {
                        serviceNames = serviceNames
                            .Where(s => s.Key.IndexOf(term, StringComparison.OrdinalIgnoreCase) == 0 || s.Value.IndexOf(term, StringComparison.OrdinalIgnoreCase) == 0);
                    }

                    return serviceNames
                        .Select(s => new { id = s.Key, name = s.Value });
                }
            }
            catch
            {
                return new object[0];
            }
        }
    }
}
