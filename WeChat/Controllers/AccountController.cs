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
        public ActionResult Index()
        {
            return View("Account");
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
    }
}