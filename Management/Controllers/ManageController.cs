using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Management.Controllers
{
    public class ManageController : BaseController
    {
        // GET: Manage
        [Authorize]
        public ActionResult Index()
        {
            return View("Manage");
        }
    }
}