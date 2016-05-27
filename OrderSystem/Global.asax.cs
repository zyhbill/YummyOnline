using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Web;
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
			var _ = new YummyOnlineManager().RecordLog(Log.LogProgram.OrderSystem, Log.LogLevel.Info, "OrderSystem Initializing");
			_ = NewDineInformTcpClient.Initialize();

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

#if !DEBUG
		protected void Application_Error(object sender, EventArgs e) {
			Exception exception = Server.GetLastError();

			Request.InputStream.Seek(0, SeekOrigin.Begin);
			string postData = new StreamReader(Request.InputStream).ReadToEnd();

			string action = "HttpError500";

			HttpException httpException = exception as HttpException;
			if(httpException != null) {
				switch(httpException.GetHttpCode()) {
					case 404:
						action = "HttpError404";
						break;
				}
			}

			AsyncInline.Run(() => new YummyOnlineManager().RecordLog(Log.LogProgram.OrderSystem, Log.LogLevel.Error,
				$"{action}: Host: {Request.UserHostAddress}, RequestUrl: {Request.RawUrl}, PostData: {postData}, Exception: {exception.Message}"));

			Response.Clear();
			Server.ClearError();
			Response.Redirect($"~/Error/{action}", true);
		}
#endif
	}
}
