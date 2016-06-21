using HotelDAO;
using HotelDAO.Models;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Text;
using System.Web;
using System.Web.Mvc;
using YummyOnlineDAO.Models;
using System.Linq;

namespace Management.Controllers
{
    public class BaseController : Controller
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
        private HotelContext _db = null;
        public HotelContext db
        {
            get
            {
                if (_db == null)
                {
                    _db = new HotelContext(Session["ConnectString"] as string);
                }
                return _db;
            }
        }
        private YummyOnlineContext _sysdb = null;
        public YummyOnlineContext sysdb
        {
            get
            {
                if (_sysdb == null)
                {
                    _sysdb = new YummyOnlineContext();
                }
                return _sysdb;
            }
        }
        private string _WebSocketUrl = null;
        public string WebSocketUrl
        {
            get
            {
                if (_WebSocketUrl == null)
                {
                    int Port  = sysdb.SystemConfigs.Select(s => s.ManagementWebSocketPort).FirstOrDefault();
                    _WebSocketUrl += "ws://0.0.0.0:" + Port.ToString();
                }
                return _WebSocketUrl;
            }
        }
    }

    public class JsonNetResult : JsonResult
    {
        public JsonSerializerSettings Settings { get; private set; }

        public JsonNetResult()
        {
            Settings = new JsonSerializerSettings
            {
                //这句是解决问题的关键,也就是json.net官方给出的解决配置选项.                 
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