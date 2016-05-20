using Protocal;
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

		public async Task<ActionResult> HttpError404(string requestUrl, string postData) {
			await YummyOnlineManager.RecordLog(Log.LogProgram.OrderSystem_Waiter, Log.LogLevel.Error, $"Error 400: RequestUrl {requestUrl}, PostData {postData}");
			if(Request.IsAjaxRequest()) {
				return Json(new JsonError("页面未找到"), JsonRequestBehavior.AllowGet);
			}
			return View();
		}
		public async Task<ActionResult> HttpError500(string requestUrl, string postData, string exception) {
			await YummyOnlineManager.RecordLog(Log.LogProgram.OrderSystem_Waiter, Log.LogLevel.Error, $"Error 500: RequestUrl {requestUrl}, PostData {postData}, Exception {exception}");
			if(Request.IsAjaxRequest()) {
				return Json(new JsonError("服务器内部错误"), JsonRequestBehavior.AllowGet);
			}
			return View();
		}
	}
}