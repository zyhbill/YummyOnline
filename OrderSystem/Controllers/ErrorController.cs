using Protocal;
using System.Threading.Tasks;
using System.Web.Mvc;
using YummyOnlineDAO.Models;

namespace OrderSystem.Controllers {
	public class ErrorController : BaseOrderSystemController {
		public ActionResult HotelMissing() {
			if(Request.IsAjaxRequest()) {
				return Json(new JsonError("没有饭店信息，请重新扫一扫"), JsonRequestBehavior.AllowGet);
			}
			return View();
		}

		public ActionResult HotelUnavailable() {
			if(Request.IsAjaxRequest()) {
				return Json(new JsonError("该饭店不可用，请重新扫一扫"), JsonRequestBehavior.AllowGet);
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