using System.Web.Mvc;
using Protocal;

namespace YummyOnline.Controllers {
	public class AuthController : BaseController {
		// GET: Auth
		public ActionResult Index(string ReturnUrl) {
			if(Request.IsAjaxRequest()) {
				return Json(new JsonError("没有权限"), JsonRequestBehavior.AllowGet);
			}
			string returnUrl = $"{nameof(ReturnUrl)}={ReturnUrl}";
			return Redirect($"/Account?{Server.UrlEncode(returnUrl)}");
		}
	}
}