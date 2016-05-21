using Protocal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using YummyOnlineDAO.Models;

namespace YummyOnline.Controllers {
	[Authorize(Roles = nameof(Role.Admin))]
	public class HotelController : BaseController {
		// GET: Hotel
		public ActionResult Index() {
			return View();
		}

		public async Task<JsonResult> GetHotelNames() {
			List<Hotel> hotels = await YummyOnlineManager.GetHotels();
			return Json(hotels.Select(p => new {
				p.Id,
				p.Name,
			}));
		}
		public async Task<JsonResult> GetHotels() {
			List<Hotel> hotels = await YummyOnlineManager.GetHotels();
			return Json(hotels.Select(p => new {
				p.Id,
				p.Name,
				p.CssThemePath,
				p.CreateDate,
				p.Tel,
				p.Address,
				p.OpenTime,
				p.CloseTime,
				p.Usable
			}));
		}
		public async Task<JsonResult> UpdateHotel(Hotel hotel) {
			await YummyOnlineManager.UpdateHotel(hotel);
			return Json(new JsonSuccess());
		}
	}
}