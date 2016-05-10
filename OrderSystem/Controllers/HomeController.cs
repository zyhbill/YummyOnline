using HotelDAO;
using HotelDAO.Models;
using System;
using System.Threading.Tasks;
using System.Web.Mvc;
using YummyOnlineDAO;
using YummyOnlineDAO.Models;

namespace OrderSystem.Controllers {
	public class HomeController : BaseOrderSystemController {
		// GET: 店小二点菜系统入口 Home/Index/[hotelId]/[qrCode]
		public async Task<ActionResult> Index(int? hotelId, string qrCode) {
			if(CurrHotel != null && Session["CurrentDesk"] != null) {
				return View();
			}

			if(hotelId == null || qrCode == null) {
				return RedirectToAction("HotelMissing", "Error");
			}

			Hotel hotel = await YummyOnlineManager.GetHotelById(Convert.ToInt32(hotelId));
			if(hotel == null) {
				return RedirectToAction("HotelMissing", "Error");
			}

			CurrHotel = hotel;

			Desk desk = await new HotelManager(hotel.ConnectionString).GetDeskByQrCode(qrCode);
			if(desk == null) {
				return RedirectToAction("HotelMissing", "Error");
			}

			Session["CurrentDesk"] = desk;
			return View();
		}

		public ActionResult _PartialNavBase() {
			return View();
		}
	}
}