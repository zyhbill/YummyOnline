using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using YummyOnlineDAO.Models;
using YummyOnline.Utility;

namespace YummyOnline.Controllers {
	[Authorize(Roles = nameof(Role.Admin))]
	public class DashBoardController : BaseController {
		public async Task<ActionResult> Index() {
			ViewBag.DiskFreeSpace = DriveSpace.GetHardDiskFreeSpace();
			ViewBag.DiskSpace = DriveSpace.GetHardDiskSpace();
			ViewBag.HotelCount = await YummyOnlineManager.GetHotelCount();
			int userCount = 0;
			userCount += await YummyOnlineManager.GetUserCount(Role.Customer);
			userCount += await YummyOnlineManager.GetUserCount(Role.Nemo);
			ViewBag.UserCount = userCount;
			ViewBag.DineCount = await YummyOnlineManager.GetDineCount();
			return View();
		}
		public async Task<JsonResult> GetUserDailyCount() {
			var result = new {
				CustomerDailyCount = await YummyOnlineManager.GetUserDailyCount(Role.Customer),
				CustomerCount = await YummyOnlineManager.GetUserCount(Role.Customer),
				NemoDailyCount = await YummyOnlineManager.GetUserDailyCount(Role.Nemo),
				NemoCount = await YummyOnlineManager.GetUserCount(Role.Nemo),
			};
			return Json(result);
		}

		public async Task<JsonResult> GetDineDailyCount() {
			List<dynamic> list = new List<dynamic>();
			List<Hotel> hotels = await YummyOnlineManager.GetHotels();
			foreach(Hotel h in hotels) {
				List<dynamic> dailyCount = new List<dynamic>();
				HotelDAO.HotelManagerForAdmin hotelManager = new HotelDAO.HotelManagerForAdmin(h.ConnectionString);
				for(int i = -30; i <= 0; i++) {
					DateTime t = DateTime.Now.AddDays(i);
					int count = await hotelManager.GetDineCount(t);
					dailyCount.Add(new {
						DateTime = t,
						Count = count
					});
				}

				list.Add(new {
					HotelName = h.Name,
					DailyCount = dailyCount
				});
			}

			return Json(list);
		}

		public async Task<JsonResult> GetDinePerHourCount(DateTime? dateTime) {
			DateTime dt = dateTime.HasValue ? dateTime.Value : DateTime.Now;

			List<dynamic> list = new List<dynamic>();
			List<Hotel> hotels = await YummyOnlineManager.GetHotels();
			foreach(Hotel h in hotels) {
				HotelDAO.HotelManagerForAdmin hotelManager = new HotelDAO.HotelManagerForAdmin(h.ConnectionString);

				list.Add(new {
					HotelName = h.Name,
					Counts = await hotelManager.GetDinePerHourCount(dt)
				});
			}

			return Json(list);
		}
	}
}