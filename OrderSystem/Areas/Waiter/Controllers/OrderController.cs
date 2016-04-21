using HotelDAO;
using HotelDAO.Models;
using OrderSystem.Models;
using System.Threading.Tasks;
using System.Web.Mvc;
using YummyOnlineDAO.Identity;
using YummyOnlineDAO.Models;
using Protocal;

namespace OrderSystem.Areas.Waiter.Controllers {
	[Authorize(Roles = nameof(Schema.ReadWaiterData))]
	public class OrderController : BaseWaiterController {
		// GET: Waiter/Order
		public ActionResult Index() {
			if(CurrHotel == null) {
				return Redirect("/Waiter/Account");
			}
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
			Hotel hotel = CurrHotel;
			if(hotel == null) {
				return Json(new JsonError("Hotel Missing"));
			}
			string connStr = hotel.ConnectionString;
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
					hotel.Name,
					hotel.Address,
					hotel.Tel,
					hotel.OpenTime,
					hotel.CloseTime
				}),
				Desks = await tDesks,
				PayKind = await tPayKind,
			};
			return Json(result);
		}

		public async Task<JsonResult> GetHotelInfos() {
			Hotel hotel = CurrHotel;
			if(hotel == null) {
				return Json(new JsonError("Hotel Missing"));
			}
			string connStr = hotel.ConnectionString;

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