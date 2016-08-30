using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Text.RegularExpressions;
using Protocol;

namespace WeChat.Controllers {
	public class AccountController : OrderSystem.Controllers.BaseAccountController
    {
        public ActionResult Index(string openid)
        {
            Session["openid"] = openid;
            return View("Account");
        }

        public ActionResult Success()
        {
            return View("Success");
        }

        public JsonResult Verify(string Phone)
        {
            string dianxin = @"^1[3578][01379]\d{8}$";
            Regex dReg = new Regex(dianxin);
            string liantong = @"^1[345678][01256]\d{8}$";
            Regex lReg = new Regex(liantong);
            string yidong = @"^134[012345678]\d{7}|1[34578][0123456789]\d{8}$";
            Regex yReg = new Regex(yidong);

            var P = new Regex("^[0-9]{11,11}$");
            if (P.IsMatch(Phone))
                return Json(new JsonSuccess());
            else
                return Json(new JsonError("号码不对"));
        }

        public ActionResult query(string phone)
        {
            string wechatid = Session["openid"].ToString();
            if (validateP(phone) == true)
                return Json(new { status = false, ErrorMessage = "已有该用户" });
            else if (validateO(wechatid) == true)
                return Json(new { status = false, ErrorMessage = "已有该用户openid" });
            else
            {
                if (wechatid == "" || wechatid == null)
                    return Json(new JsonError("openid为空"));
                var ctx = new YummyOnlineDAO.Models.YummyOnlineContext();
                var User = ctx.Users.FirstOrDefault(d => d.PhoneNumber == phone);
                if(User==null)
                {
                    return Json(new JsonError("user没有用户信息"));
                }
                else
                {
                    User.WeChatOpenId = wechatid;
                    ctx.SaveChanges();
                    return Json(new JsonSuccess());
                }
            }
        }

        bool validateO(string openid)
        {
            if (openid == null)
                return false;
            var yummonlineManager = new YummyOnlineManager();
            var ctx=new YummyOnlineDAO.Models.YummyOnlineContext();
            var result = ctx.Users.Where(p => p.WeChatOpenId == openid)
                .Select(d => new { d.UserName, d.Id, d.Email, d.PhoneNumber, d.WeChatOpenId }).FirstOrDefault();
            if (result == null)
                return false;
            else
                return true;
        }

        bool validateP(string phone)
        {
            var yummyonlineManager = new YummyOnlineManager();
            var ctx = new YummyOnlineDAO.Models.YummyOnlineContext();
            var result = ctx.Users.Where(p => p.PhoneNumber == phone)
                .Select(d => new { d.UserName, d.Id, d.Email, d.PhoneNumber, d.WeChatOpenId }).FirstOrDefault();
            if (result == null)
                return false;
            else
                return true;
        }
    }
}