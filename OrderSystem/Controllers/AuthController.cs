using OrderSystem.Models;
using System.Web.Mvc;
using Protocal;

namespace OrderSystem.Controllers {
	public class AuthController : BaseCommonController {
		// GET: Auth
		public ActionResult Index(string ReturnUrl) {
			string returnUrl = $"{nameof(ReturnUrl)}={ReturnUrl}";

			string[] postfix = new string[] {
				"","/","#","/#","#/","/#/"
			};
			string[] waiterRedirectUrl = new string[] {
				"Waiter/Order",
				"Waiter/Order/Index"
			};

			foreach(string p in postfix) {
				foreach(string u in waiterRedirectUrl) {
					if(ReturnUrl.EndsWith(u + p)) {
						return Redirect($"/Waiter/Account?{Server.UrlEncode(returnUrl)}");
					}
				}
			}

			return Json(new JsonError("没有权限"), JsonRequestBehavior.AllowGet);
		}
	}
}