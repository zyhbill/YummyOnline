using HotelDAO;
using HotelDAO.Models;
using OrderSystem.Utility;
using Protocal;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Mvc;
using Utility;
using YummyOnlineDAO.Identity;

namespace OrderSystem.Waiter.Controllers {
	[Authorize(Roles = nameof(Schema.ReadWaiterData))]
	[HotelAvailable]
	public class OrderController : BaseWaiterController {
		// GET: Order

		public ActionResult Index() {
			return View();
		}

		public ActionResult _ViewCart() {
			return View();
		}
		public ActionResult _ViewPayment() {
			return View();
		}
		public ActionResult _ViewCurrent() {
			return View();
		}

		public async Task<JsonResult> GetMenuInfos() {
			string connStr = CurrHotel.ConnectionString;
			var tMenuClasses = new HotelManager(connStr).GetMenuClasses();
			var tMenus = new HotelManager(connStr).GetMenus();
			var tMenuOnSales = new HotelManager(connStr).GetMenuOnSales();
			var tMenuSetMeals = new HotelManager(connStr).GetMenuSetMeals();
			var tHotel = new HotelManager(connStr).GetHotelConfig();
			var tTimeDiscounts = new HotelManager(connStr).GetTimeDiscounts();
			var tVipDiscounts = new HotelManager(connStr).GetVipDiscounts();

			var tPayKind = new HotelManagerForWaiter(connStr).GetPayKind();
			var tDesks = new HotelManagerForWaiter(connStr).GetDesks();

			var result = new {
				MenuClasses = await tMenuClasses,
				Menus = await tMenus,
				MenuOnSales = await tMenuOnSales,
				MenuSetMeals = await tMenuSetMeals,
				DiscountMethods = new {
					TimeDiscounts = await tTimeDiscounts,
					VipDiscounts = await tVipDiscounts
				},
				Hotel = DynamicsCombination.CombineDynamics(await tHotel, new {
					CurrHotel.Name,
					CurrHotel.Address,
					CurrHotel.Tel,
					CurrHotel.OpenTime,
					CurrHotel.CloseTime
				}),
				Desks = await tDesks,
				PayKind = await tPayKind,
			};
			return Json(result);
		}

		public async Task<JsonResult> GetHotelInfos() {
			string connStr = CurrHotel.ConnectionString;

			var tAreas = new HotelManagerForWaiter(connStr).GetAreas();
			var tPayKinds = new HotelManagerForWaiter(connStr).GetPayKinds();
			var tRemarks = new HotelManagerForWaiter(connStr).GetRemarks();

			var result = new {
				Areas = await tAreas,
				PayKinds = await tPayKinds,
				Remarks = await tRemarks
			};
			return Json(result);
		}

		public async Task<JsonResult> GetCurrentDines(string deskId) {
			return Json(await HotelManager.GetCurrentDines(User.Identity.GetUserId(), deskId));
		}

		public async Task<JsonResult> GetDineById(string dineId) {
			return Json(await HotelManager.GetDineById(dineId));
		}
	}
}