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
			return Json(await YummyOnlineManager.GetDineDailyCount());
		}
	}
}