using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WeChat.Models;
using WeChat;
using WeiPay;
using System.Web.Security;

namespace WeChat.Controllers
{
    public class LoginController : Controller
    {
        //GET: Login
       public ActionResult Login()
        {
            return View("Login");
        }

        [HttpGet]
        public ActionResult CheckSignature(string signature, string timestamp, string nonce, string echostr)
        {
            weChat t = new weChat();
            //t.Auth();
            t.ProcessMsg();
            if (signature == null || timestamp == null || nonce == null)
                return View("Login");
            string token = "wechatdianxiaoer";
            string[] tmparr = new string[] { token, timestamp, nonce };
            Array.Sort(tmparr);
            string tmpstr = string.Join("", tmparr);
            //tmpstr = Convert.ToBase64String(System.Security.Cryptography.SHA1.Create(tmpstr)?.Hash);
            tmpstr = FormsAuthentication.HashPasswordForStoringInConfigFile(tmpstr, "SHA1");
            tmpstr = tmpstr.ToLower();
            //t.subscribe();
            if (tmpstr == signature)
            {
                return Content(echostr);
            }
            else
            {
                return Content("wrong!");
            }
        }
        [HttpPost]
        public ActionResult Login1(string phone,string Paswrd)
        {
            weChat t = new weChat();
            t.Auth();
            t.ProcessMsg();
            var yummyonlineManager = new YummyOnlineManager();
            var ctx = new YummyOnlineDAO.Models.YummyOnlineContext();
            var psd = Method.GetMd5(Paswrd);
            var result = ctx.Users.Where(p => p.PhoneNumber ==phone && p.PasswordHash == psd)
                .Select(d=>new { d.UserName,d.Id,d.Email,d.PhoneNumber}).FirstOrDefault();
            int points = 0;
            var hotels = ctx.Hotels.Where(d => d.Usable == true).ToList();
            foreach(var i in hotels)
            {
                var ConnectStr = i.ConnectionString;
                var HotelManager = new HotelManager(ConnectStr);
                points += HotelManager.GetUserPointById(result.Id);
            }
            //foreach (var d in result)
            //{


            //console.writeline("name:{0}", d.username);
            //console.writeline("");
            //}
            if (result == null)
                return Json(new { Status = false ,ErrorMessage = "error"});
            else
            {
                Session["User"] = result;
                return Json(new { Status = true, user = result, points = points });//name=result.UserName,id=result.Id,email=result.Email,tel=result.PhoneNumber;
                //return View("UserInfo");

            }

        }
    }
}