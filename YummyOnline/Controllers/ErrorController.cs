using System.Threading.Tasks;
using System.Web.Mvc;
using YummyOnlineDAO.Models;
using Protocol;

namespace YummyOnline.Controllers {
	public class ErrorController : BaseController {
		// GET: Error
		public ActionResult HotelMissing() {
			return View();
		}

		public ActionResult HttpError404() {
			if(Request.IsAjaxRequest()) {
				return Json(new JsonError("页面未找到"), JsonRequestBehavior.AllowGet);
			}
			return View();
		}
		public ActionResult HttpError500() {
			if(Request.IsAjaxRequest()) {
				return Json(new JsonError("服务器内部错误"), JsonRequestBehavior.AllowGet);
			}
			return View();
		}
	}
}