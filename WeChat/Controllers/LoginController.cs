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
        // GET: Login
        [HttpGet]
        public ActionResult Login()
        {
            return View("Login");
        }

        [HttpGet]
        public bool Login(string signature,string timestamp,string nonce)
        {
            string token = "wechat_dianxiaoer";
            string[] tmparr = new string[] {token,timestamp,nonce};
            Array.Sort(tmparr);
            string tmpstr = string.Join("", tmparr);
            tmpstr = Convert.ToBase64String(System.Security.Cryptography.SHA1.Create(tmpstr).Hash);
            tmpstr = tmpstr.ToLower();
            if(tmpstr==signature)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        //[HttpPost]
        public JsonResult Login1(string phone,string Paswrd)
        {
            weChat t = new weChat();
            t.Auth();
            t.ProcessMsg();
            var yummyonlineManager = new YummyOnlineManager();
            var ctx = new YummyOnlineDAO.Models.YummyOnlineContext();
            var psd = Method.GetMd5(Paswrd);
            var result = ctx.Users.Where(p => p.PhoneNumber ==phone && p.PasswordHash == psd)
                .Select(d=>new { d.UserName,d.Id,d.Email,d.PhoneNumber}).FirstOrDefault();

            //foreach (var d in result)
            //{


            //console.writeline("name:{0}", d.username);
            //console.writeline("");
            //}
            if (result == null)
                return Json(new { Status = false });
            else
                return Json(new { Status = true ,user=result});//name=result.UserName,id=result.Id,email=result.Email,tel=result.PhoneNumber;
            
        }
    }
}