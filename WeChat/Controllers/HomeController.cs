using HotelDAO;
using HotelDAO.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Mvc;
using YummyOnlineDAO;
using YummyOnlineDAO.Models;

namespace WeChat.Controllers
{
    public class HomeController : BaseWeChartController
    {
        public ActionResult Index()
        {


            YummyOnlineManager manager = new YummyOnlineManager();
            var ids = manager.GetHotelIds();
            

            return Json(ids, JsonRequestBehavior.AllowGet);
        }
    }
}