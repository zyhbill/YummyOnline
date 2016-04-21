using System;
using System.Security.Principal;
using System.Threading.Tasks;
using System.Web;
using System.Web.Security;
using YummyOnlineDAO.Models;
using HotelDAO;
using HotelDAO.Models;
using System.Collections.Generic;

namespace YummyOnlineDAO.Identity {
	public class BaseSigninManager {
		public BaseSigninManager(HttpContextBase httpCtx) {
			this.httpCtx = httpCtx;
		}
		protected HttpContextBase httpCtx;

		public void Signout() {
			FormsAuthentication.SignOut();
		}
	}

	public class SigninManager : BaseSigninManager {
		public SigninManager(HttpContextBase httpCtx) : base(httpCtx) {

		}

		/// <summary>
		/// 普通用户登录
		/// </summary>
		/// <param name="user"></param>
		/// <param name="isPersistent"></param>
		/// <returns></returns>
		public async Task Signin(User user, bool isPersistent) {
			// 添加用户的权限
			Role[] roles = (await new UserManager().GetRolesAsync(user.Id)).ToArray();
			string userData = String.Join(",", roles);

			// 将用户Id作为票据的Name
			FormsAuthenticationTicket ticket = new FormsAuthenticationTicket(1, user.Id.ToString(), DateTime.Now,
				DateTime.Now.AddDays(7),
				isPersistent, userData);
			string authTicket = FormsAuthentication.Encrypt(ticket);
			HttpCookie cookie = new HttpCookie(FormsAuthentication.FormsCookieName, authTicket);
			if(isPersistent) {
				cookie.Expires = ticket.Expiration;
			}

			httpCtx.Response.SetCookie(cookie);
		}

		public async Task<bool> IsAuthenticated() {
			if(!httpCtx.User.Identity.IsAuthenticated) {
				return false;
			}
			UserManager userManager = new UserManager();
			User user = await userManager.FindByIdAsync(httpCtx.User.Identity.GetUserId());

			if(user == null || await userManager.IsInRoleAsync(user.Id, Role.Nemo)) {
				return false;
			}

			return true;
		}
	}

	public static class IdentityExtensions {
		public static string GetUserId(this IIdentity identity) {
			return identity.IsAuthenticated ? identity.Name : null;
		}
	}
}
