using System;
using System.Web;
using System.Web.Mvc;
using Utility;
using YummyOnlineDAO;
using YummyOnlineDAO.Models;

namespace OrderSystem {
	public class AppHandleErrorAttribute : HandleErrorAttribute {
		public override void OnException(ExceptionContext filterContext) {
			Exception exception = filterContext.Exception;

			AsyncInline.Run(() => new YummyOnlineManager().RecordLog(Log.LogProgram.OrderSystem, Log.LogLevel.Error,
				$"Host: {HttpContext.Current.Request.UserHostAddress}, RequestUrl: {HttpContext.Current.Request.RawUrl}, Message: {exception.Message}",
				$"PostData: {HttpPost.GetPostData(HttpContext.Current.Request)}, Exception: {exception}"));

			filterContext.ExceptionHandled = true;
			filterContext.Result = new RedirectResult($"~/Error/HttpError500");
		}
	}

	public class FilterConfig {
		public static void RegisterGlobalFilters(GlobalFilterCollection filters) {
#if !DEBUG
			filters.Add(new AppHandleErrorAttribute());
#endif
		}
	}
}
