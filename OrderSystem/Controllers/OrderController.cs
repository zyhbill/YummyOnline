using HotelDAO;
using HotelDAO.Models;
using System.Threading.Tasks;
using System.Web.Mvc;
using YummyOnlineDAO.Identity;
using YummyOnlineDAO.Models;
using Protocal;
using YummyOnlineDAO;
using System.Collections.Generic;
using OrderSystem.Models;
using OrderSystem.Utility;

namespace OrderSystem.Controllers {
	public class OrderController : BaseCommonController {
		// GET: Order
		public ActionResult Index() {
			if(CurrHotel == null) {
				return RedirectToAction("HotelMissing", "Error");
			}
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
			Hotel hotel = CurrHotel;
			if(hotel == null) {
				return Json(new JsonError("Hotel Missing"));
			}
			var t1 = new HotelManager(hotel.ConnectionString).GetMenuClasses();
			var t2 = new HotelManager(hotel.ConnectionString).GetMenus();
			var t3 = new HotelManager(hotel.ConnectionString).GetMenuOnSales();
			var t4 = new HotelManager(hotel.ConnectionString).GetMenuSetMeals();
			var t5 = new HotelManager(hotel.ConnectionString).GetPayKinds();
			var t6 = new HotelManager(hotel.ConnectionString).GetHotelConfig();
			var t7 = new HotelManager(hotel.ConnectionString).GetTimeDiscounts();
			var t8 = new HotelManager(hotel.ConnectionString).GetVipDiscounts();

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
					hotel.Name,
					hotel.Address,
					hotel.Tel,
					hotel.OpenTime,
					hotel.CloseTime
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
			HotelManagerForTcpServer hotelManager4Tcp = new HotelManagerForTcpServer(dpProtocal.Hotel.ConnectionString);

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