using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using System.Web.Security;
using Utility;
using YummyOnlineDAO.Identity;
using YummyOnlineDAO.Models;

namespace YummyOnline {
	public class MvcApplication : HttpApplication {
		protected void Application_Start() {
			AreaRegistration.RegisterAllAreas();
			FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
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

		protected void Application_Error(object sender, EventArgs e) {
			Exception exception = Server.GetLastError();
			Response.Clear();
			Server.ClearError();

			Stream s = Request.InputStream;
			byte[] b = new byte[s.Length];
			s.Read(b, 0, (int)s.Length);
			string postData = Encoding.UTF8.GetString(b);

			string action = "HttpError500";

			HttpException httpException = exception as HttpException;
			if(httpException != null) {
				switch(httpException.GetHttpCode()) {
					case 404:
						action = "HttpError404";
						break;
				}
			}

			Response.Redirect($"~/Error/{action}?RequestUrl={HttpUtility.UrlEncode(Request.RawUrl)}&PostData={HttpUtility.UrlEncode(postData)}&Exception={exception.Message}", true);
		}
	}
}
