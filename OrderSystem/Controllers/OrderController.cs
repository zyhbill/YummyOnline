using HotelDAO;
using HotelDAO.Models;
using Protocal;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Mvc;
using Utility;
using YummyOnlineDAO;
using YummyOnlineDAO.Identity;

namespace OrderSystem.Controllers {
	[RequireHotel]
	[HotelAvailable]
	public class OrderController : BaseOrderSystemController {
		// GET: Order
		public ActionResult Index() {
			return View();
		}

		public ActionResult _ViewMenu() {
			return View();
		}
		public ActionResult _ViewCart() {
			return View();
		}
		public ActionResult _ViewPayment() {
			return View();
		}
		public ActionResult _ViewHistory() {
			return View();
		}

		public JsonResult GetCurrentDesk() {
			return Json(Session["CurrentDesk"]);
		}

		public async Task<JsonResult> GetMenuInfos() {
			var t1 = new HotelManager(CurrHotel.ConnectionString).GetMenuClasses();
			var t2 = new HotelManager(CurrHotel.ConnectionString).GetMenus();
			var t3 = new HotelManager(CurrHotel.ConnectionString).GetMenuOnSales();
			var t4 = new HotelManager(CurrHotel.ConnectionString).GetMenuSetMeals();
			var t5 = new HotelManager(CurrHotel.ConnectionString).GetPayKinds(new List<PayKindType> { PayKindType.Online, PayKindType.Other });
			var t6 = new HotelManager(CurrHotel.ConnectionString).GetHotelConfig();
			var t7 = new HotelManager(CurrHotel.ConnectionString).GetTimeDiscounts();
			var t8 = new HotelManager(CurrHotel.ConnectionString).GetVipDiscounts();

			var result = new {
				MenuClasses = await t1,
				Menus = await t2,
				MenuOnSales = await t3,
				MenuSetMeals = await t4,
				PayKinds = await t5,
				DiscountMethods = new {
					TimeDiscounts = await t7,
					VipDiscounts = await t8
				},
				Hotel = DynamicsCombination.CombineDynamics(await t6, new {
					CurrHotel.Name,
					CurrHotel.Address,
					CurrHotel.Tel,
					CurrHotel.OpenTime,
					CurrHotel.CloseTime
				})
			};
			return Json(result);
		}

		public async Task<JsonResult> GetHistoryDines() {
			return Json(await HotelManager.GetHistoryDines(User.Identity.GetUserId()));
		}

		public async Task<JsonResult> GetDineForPrinting(int hotelId, string dineId) {
			DineForPrintingProtocal dpProtocal = new DineForPrintingProtocal();

			dpProtocal.Hotel = await YummyOnlineManager.GetHotelById(hotelId);
			HotelManager hotelManager4Tcp = new HotelManager(dpProtocal.Hotel.ConnectionString);

			dpProtocal.Dine = await hotelManager4Tcp.GetDineById(dineId);
			dpProtocal.User = await new UserManager().FindByIdAsync(dpProtocal.Dine.UserId);

			foreach(DineMenu dineMenu in dpProtocal.Dine.DineMenus) {
				if(dineMenu.Menu.IsSetMeal) {
					List<MenuSetMeal> list = await hotelManager4Tcp.GetMenuSetMealByMenuSetId(dineMenu.MenuId);
					List<DineForPrintingProtocal.SetMealMenu> setMealInfos = new List<DineForPrintingProtocal.SetMealMenu>();
					list.ForEach(p => {
						setMealInfos.Add(new DineForPrintingProtocal.SetMealMenu {
							Name = p.Menu.Name,
							Count = p.Count
						});
					});
					dpProtocal.SetMeals.Add(dineMenu.Menu.Id, setMealInfos);
				}
			}
			return Json(dpProtocal);
		}
	}
}