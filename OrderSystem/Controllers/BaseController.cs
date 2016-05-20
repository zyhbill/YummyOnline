using HotelDAO;
using Newtonsoft.Json;
using OrderSystem.Models;
using System;
using System.IO;
using System.Text;
using System.Web;
using System.Web.Mvc;
using YummyOnlineDAO;
using YummyOnlineDAO.Identity;
using YummyOnlineDAO.Models;
using Protocal;
using System.Web.Routing;
using Utility;

namespace OrderSystem.Controllers {
	public class BaseController : Controller {
		protected override JsonResult Json(object data, string contentType, Encoding contentEncoding, JsonRequestBehavior behavior) {
			return new JsonNetResult {
				Data = data,
				ContentType = contentType,
				ContentEncoding = contentEncoding,
				JsonRequestBehavior = behavior
			};
		}
		protected override void OnActionExecuted(ActionExecutedContext filterContext) {
			ViewBag.HotelId = CurrHotel?.Id;
			ViewBag.CssThemePath = CurrHotel?.CssThemePath;

			base.OnActionExecuted(filterContext);
		}
		private YummyOnlineManager _yummyOnlineManager;
		public YummyOnlineManager YummyOnlineManager {
			get {
				if(_yummyOnlineManager == null) {
					_yummyOnlineManager = new YummyOnlineManager();
				}
				return _yummyOnlineManager;
			}
		}

		public Hotel CurrHotel {
			get {
				return (Hotel)Session["Hotel"];
			}
			set {
				Session["Hotel"] = value;
			}
		}

		protected override void OnActionExecuting(ActionExecutingContext filterContext) {
			if(CurrHotel != null) {
				Hotel hotel = AsyncInline.Run(() => YummyOnlineManager.GetHotelById(CurrHotel.Id));
				if(!hotel.Usable) {
					CurrHotel = null;
					filterContext.Result = RedirectToAction("HotelUnavailable", "Error", null);
					return;
				}
			}

			base.OnActionExecuting(filterContext);
		}
	}

	public class BaseOrderSystemController : BaseController {
		private UserManager _userManager;
		public UserManager UserManager {
			get {
				if(_userManager == null) {
					_userManager = new UserManager();
				}
				return _userManager;
			}
		}
		private SigninManager _signinManager;
		public SigninManager SigninManager {
			get {
				if(_signinManager == null) {
					_signinManager = new SigninManager(HttpContext);
				}
				return _signinManager;
			}
		}

		private HotelManager _hotelManager;
		public HotelManager HotelManager {
			get {
				if(_hotelManager == null) {
					_hotelManager = new HotelManager(CurrHotel.ConnectionString);
				}
				return _hotelManager;
			}
		}
	}

	public class JsonNetResult : JsonResult {
		public JsonSerializerSettings Settings { get; private set; }

		public JsonNetResult() {
			Settings = new JsonSerializerSettings {
				ReferenceLoopHandling = ReferenceLoopHandling.Ignore
			};
		}

		public override void ExecuteResult(ControllerContext context) {
			if(context == null)
				throw new ArgumentNullException("context");
			if(this.JsonRequestBehavior == JsonRequestBehavior.DenyGet && string.Equals(context.HttpContext.Request.HttpMethod, "GET", StringComparison.OrdinalIgnoreCase))
				throw new InvalidOperationException("JSON GET is not allowed");
			HttpResponseBase response = context.HttpContext.Response;
			response.ContentType = string.IsNullOrEmpty(this.ContentType) ? "application/json" : this.ContentType;
			if(this.ContentEncoding != null)
				response.ContentEncoding = this.ContentEncoding;
			if(this.Data == null)
				return;
			var scriptSerializer = JsonSerializer.Create(this.Settings);
			using(var sw = new StringWriter()) {
				scriptSerializer.Serialize(sw, this.Data);
				response.Write(sw.ToString());
			}
		}
	}

	/// <summary>
	/// 必须有饭店信息Session
	/// </summary>
	public class RequireHotelAttribute : ActionFilterAttribute {
		public override void OnActionExecuting(ActionExecutingContext filterContext) {
			HttpContextBase context = filterContext.HttpContext;
			if(context.Session["Hotel"] == null) {
				filterContext.Result = new RedirectResult("/Error/HotelMissing");
				return;
			}

			base.OnActionExecuting(filterContext);
		}
	}
}