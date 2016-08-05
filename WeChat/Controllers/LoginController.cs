using System.Linq;
using System.Web.Mvc;
using WeChat.Models;
using Protocol;
using System.Diagnostics;
using System.Collections.Specialized;
using System.Web;

namespace WeChat.Controllers
{
    public class LoginController : BaseWeChartController
    {
        //GET: Login
       [HttpGet]
       public ActionResult Login(string openid)
        {
            Session["openid"] = openid;
            //Request.QueryString["openid"].ToString();
            return View("Login");
        }

        public ActionResult query(string phone,string Paswrd)
        {
            string wechatid = Session["openid"].ToString();
            //if (wechatid == "" || wechatid == null)
            //    Debug.WriteLine("0");
            //else
            //    Debug.WriteLine(wechatid);

            if (validate(phone, Paswrd)==false)
                return Json(new { Status = false, ErrorMessage = "没有用户信息" });
            else
            {
                if (wechatid == "" || wechatid == null)
                    return Json(new JsonError("openid为空"));
                var ctx = new YummyOnlineDAO.Models.YummyOnlineContext();
                //var User = ctx.Users.FirstOrDefault(d => d.WeChatOpenId == wechatid);
                var User = ctx.Users.FirstOrDefault(d => d.PhoneNumber == phone);
                if (User == null)
                {
                    return Json(new { Status = false, ErrorMessage = "user没有用户信息" });
                }
                else
                {
                    User.WeChatOpenId = wechatid;
                    ctx.SaveChanges();
                    return Json(new JsonSuccess());
                }
            }
                
        }

        bool validate(string openid)
        {
            var yummyonlineManager = new YummyOnlineManager();
            var ctx = new YummyOnlineDAO.Models.YummyOnlineContext();
            var result = ctx.Users.Where(p => p.WeChatOpenId == openid)
                .Select(d => new { d.UserName, d.Id, d.Email, d.PhoneNumber, d.WeChatOpenId }).FirstOrDefault();
            if (result == null)
                return false;
            else
                return true;
        }
        bool validate(string phone, string Paswrd)
        {
            var yummyonlineManager = new YummyOnlineManager();
            var ctx = new YummyOnlineDAO.Models.YummyOnlineContext();
            var psd = Method.GetMd5(Paswrd);
            var result = ctx.Users.Where(p => p.PhoneNumber == phone && p.PasswordHash == psd)
                .Select(d => new { d.UserName, d.Id, d.Email, d.PhoneNumber, d.WeChatOpenId }).FirstOrDefault();
            if (result == null)
                return false;
            else
                return true;
        }
    }
}