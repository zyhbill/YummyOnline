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
		public void Signin(User user, bool isPersistent) {
			FormsAuthentication.SetAuthCookie(user.Id.ToString(), isPersistent);
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
