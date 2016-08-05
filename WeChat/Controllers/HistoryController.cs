using Protocol;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace WeChat.Controllers
{
    public class HistoryController : Controller
    {
        private string _WeChetId = null;
        public string WeChetId
        {
            get
            {
                if (_WeChetId == null)
                {
                    _WeChetId = Session["openid"] as string ;
                }
                return _WeChetId;
            }
        }

        // GET: History
        [HttpGet]
        public ActionResult History(string openid)
        {
            Session["openid"] = openid;
            return View("History");
        }

        public async Task<ActionResult> select()
        {
           
            if (query(WeChetId) == true)
            {
                var Result = await history(WeChetId);
                return Json(new JsonSuccess(Result.Data));
            }
            else
                return Json(new JsonError("该用户不存在"));
        }

        //历史订单
        public async Task<JsonResult> history(string wechatid)
        {
            var yummonlineManager = new YummyOnlineManager();
            var ctx = new YummyOnlineDAO.Models.YummyOnlineContext();
            var result = ctx.Users.Where(p => p.WeChatOpenId == wechatid).Select(d => new { d.UserName, d.Id }).FirstOrDefault();
            var hotels = ctx.Hotels.Where(d => d.Usable == true).ToList();
            dynamic DineLists = new List<dynamic>();
            foreach (var i in hotels)
            {
                var ConnectStr = i.ConnectionString;
                var HotelManager = new HotelManager(ConnectStr);
                var Dines = await HotelManager.GetFormatedHistoryDines(result.Id);
                if (Dines.Count > 0)
                {
                    foreach(var j in Dines)
                    {
                        DineLists.Add(j);
                    }
                }
                Debug.WriteLine(Dines);
            }
            return Json(new JsonSuccess(DineLists));
        }

        //查询用户
        bool query(string wechatid)
        {
            var yummonlineManager = new YummyOnlineManager();
            var ctx = new YummyOnlineDAO.Models.YummyOnlineContext();
            var result = ctx.Users.Where(p => p.WeChatOpenId == wechatid).Select(d => new { d.UserName, d.Id }).FirstOrDefault();
            if (result == null)
                return false;
            else
                return true;
        }
    }
}