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

namespace Inedo.BuildMasterExtensions.Windows.Iis
{
    internal sealed class AppPoolSelector : HiddenField
    {
        protected override void OnPreRender(EventArgs e)
        {
            this.IncludeClientResourceInPage(
                new JavascriptResource
                {
                    ResourcePath = "/extension-resources/Windows/IIS/AppPoolSelector.js?" + typeof(AppPoolSelector).Assembly.GetName().Version,
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
            writer.Write("BmAppPoolSelector(");
            InedoLib.Util.JavaScript.WriteJson(
                writer,
                new
                {
                    inputSelector = "#" + this.ClientID,
                    getAppPoolsUrl = DynamicHttpHandling.GetJavascriptDataUrl<int, string, object>(AjaxGetAppPools)
                }
            );
            writer.Write(");");
            writer.Write("</script>");
        }

        [AjaxMethod]
        public static object AjaxGetAppPools(int serverId, string term = null)
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
                    IEnumerable<string> appPoolNames = (string[])remote.InvokeMethod(new Func<string[]>(ProxiedUtil.GetAppPoolNames).Method, null, null);

                    if (!string.IsNullOrWhiteSpace(term))
                    {
                        appPoolNames = appPoolNames
                            .Where(a => a.IndexOf(term, StringComparison.OrdinalIgnoreCase) == 0);
                    }

                    return appPoolNames;
                }
            }
            catch
            {
                return new object[0];
            }
        }
    }
}
