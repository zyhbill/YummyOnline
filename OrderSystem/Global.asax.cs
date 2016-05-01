using System;
using System.Net;
using System.Security.Principal;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using System.Web.Security;
using YummyOnlineDAO;
using YummyOnlineDAO.Identity;
using YummyOnlineDAO.Models;
using OrderSystem.Utility;
using System.Collections.Generic;
using Utility;

namespace OrderSystem {
	public class MvcApplication : HttpApplication {
		protected virtual void Application_Start() {
			var _ = new YummyOnlineManager().RecordLog(Log.LogProgram.OrderSystem, Log.LogLevel.Info, "OrderSystem Initializing");
			_ = NewDineInformTcpClient.Initialize();

			RouteConfig.RegisterRoutes(RouteTable.Routes);
			BundleConfig.RegisterBundles(BundleTable.Bundles);
		}

		protected void Application_AuthenticateRequest(Object sender, EventArgs e) {
			//Construst the GeneralPrincipal and FormsIdentity objects
			var authCookie = Context.Request.Cookies[FormsAuthentication.FormsCookieName];
			if(null == authCookie) {
				//no authentication cokie present
				return;
			}
			var authTicket = FormsAuthentication.Decrypt(authCookie.Value);
			if(null == authTicket) {
				//could not decrypt cookie
				return;
			}

			//get the role
			List<Role> roles = AsyncInline.Run(() => new UserManager().GetRolesAsync(authTicket.Name));

			List<string> roleStrs = new List<string>();
			roles.ForEach(r => {
				roleStrs.Add(r.ToString());
			});

			var id = new FormsIdentity(authTicket);
			Context.User = new GenericPrincipal(id, roleStrs.ToArray());
		}
	}
}
