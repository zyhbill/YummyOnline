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

			if(User.IsInRole(nameof(Role.SuperAdmin))) {
				return Json(hotels.Select(p => new {
					p.Id,
					p.Name,
					p.ConnectionString,
					p.AdminConnectionString,
					p.CssThemePath,
					p.CreateDate,
					p.Tel,
					p.Address,
					p.OpenTime,
					p.CloseTime,
					p.Usable
				}));
			}
			return Json(hotels.Select(p => new {
				p.Id,
				p.Name,
				p.ConnectionString,
				p.CssThemePath,
				p.CreateDate,
				p.Tel,
				p.Address,
				p.OpenTime,
				p.CloseTime,
				p.Usable
			}));
		}
		public async Task<JsonResult> UpdateHotelUsable(Hotel hotel) {
			await YummyOnlineManager.UpdateHotel(hotel);
			Log.LogLevel level = hotel.Usable ? Log.LogLevel.Success : Log.LogLevel.Warning;
			await YummyOnlineManager.RecordLog(Log.LogProgram.System, level, $"Set Hotel {hotel.Id} Usable {hotel.Usable}");
			return Json(new JsonSuccess());
		}
	}
}