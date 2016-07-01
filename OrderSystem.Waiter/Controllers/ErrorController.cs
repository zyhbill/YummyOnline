using Protocol;
using System.Threading.Tasks;
using System.Web.Mvc;
using YummyOnlineDAO.Models;

namespace OrderSystem.Waiter.Controllers {
	public class ErrorController : BaseWaiterController {
		public ActionResult HotelUnavailable() {
			if(Request.IsAjaxRequest()) {
				return Json(new JsonError("该饭店不可用，请联系管理员"), JsonRequestBehavior.AllowGet);
			}
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