using HotelDAO;
using HotelDAO.Models;
using OrderSystem.Utility;
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
using YummyOnlineDAO;
using YummyOnlineDAO.Identity;

namespace OrderSystem.Waiter {
	public class MvcApplication : OrderSystem.MvcApplication {
		protected override void Application_Start() {
			RouteConfig.RegisterRoutes(RouteTable.Routes);
			BundleConfig.RegisterBundles(BundleTable.Bundles);
		}

		protected override void Application_AuthenticateRequest(object sender, EventArgs e) {
			var authCookie = Context.Request.Cookies[FormsAuthentication.FormsCookieName];
			if(null == authCookie) {
				return;
			}
			var authTicket = FormsAuthentication.Decrypt(authCookie.Value);
			if(null == authTicket) {
				return;
			}

			YummyOnlineDAO.Models.Staff staff = AsyncInline.Run(() => new StaffManager().FindStaffById(authTicket.Name));
			if(staff == null) {
				FormsAuthentication.SignOut();
				return;
			}
			string connStr = AsyncInline.Run(() => new YummyOnlineManager().GetHotelConnectionStringById(staff.HotelId));
			HotelManagerForWaiter hotelManager4Waiter = new HotelManagerForWaiter(connStr);
			List<StaffRoleSchema> schemas = AsyncInline.Run(() => hotelManager4Waiter.GetStaffRoles(staff.Id));

			string[] roleStrs = schemas.Select(p => p.Schema.ToString()).ToArray();

			var id = new FormsIdentity(authTicket);
			Context.User = new GenericPrincipal(id, roleStrs);
		}
	}
}
