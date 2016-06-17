using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Principal;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using System.Web.Security;
using Utility;
using YummyOnlineDAO;
using YummyOnlineDAO.Identity;
using YummyOnlineDAO.Models;

namespace OrderSystem {
	public class MvcApplication : HttpApplication {
		protected void Application_Start() {
			YummyOnlineManager manager = new YummyOnlineManager();
			var _ = manager.RecordLog(Log.LogProgram.OrderSystem, Log.LogLevel.Info, "OrderSystem Initializing");
			_ = NewDineInformTcpClient.Initialize(async () => {
				await manager.RecordLog(Log.LogProgram.OrderSystem, Log.LogLevel.Success, "TcpServer Connected");
			}, async e => {
				await manager.RecordLog(Log.LogProgram.OrderSystem, Log.LogLevel.Error, e.Message);
			});

			FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
			RouteConfig.RegisterRoutes(RouteTable.Routes);
			BundleConfig.RegisterBundles(BundleTable.Bundles);
		}

		protected void Application_AuthenticateRequest(object sender, EventArgs e) {
			var authCookie = Context.Request.Cookies[FormsAuthentication.FormsCookieName];
			if(null == authCookie) {
				return;
			}
			var authTicket = FormsAuthentication.Decrypt(authCookie.Value);
			if(null == authTicket) {
				return;
			}

			List<Role> roles = AsyncInline.Run(() => new UserManager().GetRolesAsync(authTicket.Name));
			if(roles.Count == 0) {
				FormsAuthentication.SignOut();
				return;
			}

			string[] roleStrs = roles.Select(p => p.ToString()).ToArray();

			var id = new FormsIdentity(authTicket);
			Context.User = new GenericPrincipal(id, roleStrs);
		}
	}
}
