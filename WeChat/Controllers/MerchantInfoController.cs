using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using YummyOnlineDAO.Models;

namespace WeChat.Controllers
{
    public class MerchantInfoController : Controller
    {
        public ActionResult Index()
        {
            return View("MerchantInfo");
        }

        // GET: MerchantInfo
        public ActionResult Info(int article)
        {
            Session["id"] = article;
            var yummonlineManager = new YummyOnlineManager();
            var ctx = new YummyOnlineContext();
            var result = ctx.Articles.Where(d => d.Id == article && d.Status == ArticleStatus.Granted).Select(s => new { s.Body,s.Title,s.Description});
            foreach (var i in result)
            {
                ViewData["body"] = i.Body;
                ViewData["title"] = i.Title;
                ViewData["des"] = i.Description;
            }
            return View("Info");
        }
    }
}