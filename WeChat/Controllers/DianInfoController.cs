using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace WeChat.Controllers
{
    public class DianInfoController : Controller
    {
        // GET: DianInfo
        public ActionResult DianInfo1()
        {
            return View("DianInfo1");
        }
        public ActionResult DianInfo2()
        {
            return View("DianInfo2");
        }
    }
}