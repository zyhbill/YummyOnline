using HotelDAO;
using HotelDAO.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web;
using System.Web.Security;

namespace YummyOnlineDAO.Identity {
	public class StaffSigninManager : BaseSigninManager {
		public StaffSigninManager(HttpContextBase httpCtx) : base(httpCtx) {

		}
		public async Task Signin(Models.Staff staff, bool isPersistent) {
			string connStr = await new YummyOnlineManager().GetHotelConnectionStringById(staff.HotelId);
			HotelManagerForWaiter hotelManager4Waiter = new HotelManagerForWaiter(connStr);
			List<StaffRoleSchema> schemas = await hotelManager4Waiter.GetStaffRoles(staff.Id);

			List<string> roleStrs = new List<string>();
			schemas?.ForEach(schema => {
				roleStrs.Add(schema.Schema.ToString());
			});
			string userData = String.Join(",", roleStrs);

			// 将用户Id作为票据的Name
			FormsAuthenticationTicket ticket = new FormsAuthenticationTicket(1, staff.Id.ToString(), DateTime.Now,
				DateTime.Now.AddDays(7),
				isPersistent, userData);
			string authTicket = FormsAuthentication.Encrypt(ticket);
			HttpCookie cookie = new HttpCookie(FormsAuthentication.FormsCookieName, authTicket);
			if(isPersistent) {
				cookie.Expires = ticket.Expiration;
			}

			httpCtx.Response.SetCookie(cookie);
		}
	}
}
