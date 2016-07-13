using Newtonsoft.Json;
using System;
using System.IO;
using System.Text;
using System.Web;
using System.Web.Mvc;

using YummyOnlineDAO.Identity;
using YummyOnlineDAO.Models;

namespace WeChat.Controllers
{
    public class BaseWeChartController : Controller
    {
        protected override JsonResult Json(object data, string contentType, Encoding contentEncoding, JsonRequestBehavior behavior)
        {
            return new JsonNetResult
            {
                Data = data,
                ContentType = contentType,
                ContentEncoding = contentEncoding,
                JsonRequestBehavior = behavior
            };
        }

        private YummyOnlineManager _yummyOnlineManager;
        public YummyOnlineManager YummyOnlineManager
        {
            get
            {
                if (_yummyOnlineManager == null)
                {
                    _yummyOnlineManager = new YummyOnlineManager();
                }
                return _yummyOnlineManager;
            }
        }

        public class JsonNetResult : JsonResult
        {
            public JsonSerializerSettings Settings { get; private set; }

            public JsonNetResult()
            {
                Settings = new JsonSerializerSettings
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                };
            }

            public override void ExecuteResult(ControllerContext context)
            {
                if (context == null)
                    throw new ArgumentNullException("context");
                if (this.JsonRequestBehavior == JsonRequestBehavior.DenyGet && string.Equals(context.HttpContext.Request.HttpMethod, "GET", StringComparison.OrdinalIgnoreCase))
                    throw new InvalidOperationException("JSON GET is not allowed");
                HttpResponseBase response = context.HttpContext.Response;
                response.ContentType = string.IsNullOrEmpty(this.ContentType) ? "application/json" : this.ContentType;
                if (this.ContentEncoding != null)
                    response.ContentEncoding = this.ContentEncoding;
                if (this.Data == null)
                    return;
                var scriptSerializer = JsonSerializer.Create(this.Settings);
                using (var sw = new StringWriter())
                {
                    scriptSerializer.Serialize(sw, this.Data);
                    response.Write(sw.ToString());
                }
            }
        }
    }
}