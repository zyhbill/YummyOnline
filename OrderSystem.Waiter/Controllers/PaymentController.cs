using HotelDAO;
using HotelDAO.Models;
using Newtonsoft.Json;
using OrderSystem.Models;
using Protocal;
using System.Threading.Tasks;
using System.Web.Mvc;
using Utility;
using YummyOnlineDAO.Identity;
using YummyOnlineDAO.Models;

namespace OrderSystem.Waiter.Controllers {
	[Authorize(Roles = nameof(Schema.SubmitWaiterPay))]
	[HotelAvailable]
	public class PaymentController : BaseWaiterController {

		private UserManager _userManager;
		public UserManager UserManager {
			get {
				if(_userManager == null) {
					_userManager = new UserManager();
				}
				return _userManager;
			}
		}


		private OrderManager _orderManager;
		public OrderManager OrderManager {
			get {
				if(_orderManager == null) {
					_orderManager = new OrderManager(CurrHotel.ConnectionString);
				}
				return _orderManager;
			}
		}

		public async Task<JsonResult> WaiterPay(Cart cart, WaiterCartAddition cartAddition) {
			FunctionResult result = await waiterPay(cart, cartAddition);
			if(!result.Succeeded) {
				return Json(new JsonError(result.Message));
			}

			Dine dine = ((Dine)result.Data);
			await newDineInform(dine, "Waiter");
			return Json(new JsonSuccess { Data = dine.Id });
		}

		public async Task<JsonResult> WaiterPayCompleted(WaiterPaidDetails paidDetails) {
			bool succeeded = await OrderManager.OfflinePayCompleted(paidDetails);
			if(!succeeded) {
				return Json(new JsonError("支付金额与应付金额不匹配"));
			}

			NewDineInformTcpClient.SendNewDineInfrom(CurrHotel.Id, paidDetails.DineId, true);
			return Json(new JsonSuccess { Data = paidDetails.DineId });
		}

		public async Task<JsonResult> WaiterPayWithPaidDetails(Cart cart, WaiterCartAddition cartAddition, WaiterPaidDetails paidDetails) {
			FunctionResult result = await waiterPay(cart, cartAddition);
			if(!result.Succeeded) {
				return Json(new JsonError(result.Message));
			}

			Dine dine = ((Dine)result.Data);
			await newDineInform(dine, "WaiterWithPaidDetail");
			paidDetails.DineId = dine.Id;

			bool succeeded = await OrderManager.OfflinePayCompleted(paidDetails);
			if(!succeeded) {
				return Json(new JsonError("支付金额与应付金额不匹配"));
			}

			NewDineInformTcpClient.SendNewDineInfrom(CurrHotel.Id, paidDetails.DineId, true);
			return Json(new JsonSuccess { Data = paidDetails.DineId });
		}

		private async Task<FunctionResult> waiterPay(Cart cart, WaiterCartAddition cartAddition) {
			HotelManagerForWaiter hotelManager = new HotelManagerForWaiter(CurrHotel.ConnectionString);
			cart.PayKindId = await hotelManager.GetOtherPayKindId();
			CartAddition addition = new CartAddition {
				WaiterId = User.Identity.Name,
				Discount = cartAddition.Discount,
				DiscountName = cartAddition.DiscountName
			};

			User user = await UserManager.FindByIdAsync(cartAddition.UserId);
			if(user == null) {
				user = await UserManager.CreateVoidUserAsync();
				if(user == null) {
					return new FunctionResult(false, "创建匿名用户失败");
				}
				await YummyOnlineManager.RecordLog(YummyOnlineDAO.Models.Log.LogProgram.Identity, YummyOnlineDAO.Models.Log.LogLevel.Success, $"Anonymous User Created {user.Id} From WaiterPay");
				await UserManager.AddToRoleAsync(user.Id, Role.Nemo);
			}
			addition.UserId = user.Id;

			// 创建新订单
			FunctionResult result = await OrderManager.CreateDine(cart, addition);
			if(!result.Succeeded) {
				await HotelManager.RecordLog(HotelDAO.Models.Log.LogLevel.Error, result.Detail);
			}

			return result;
		}

		private async Task newDineInform(Dine dine, string via) {
			await HotelManager.RecordLog(HotelDAO.Models.Log.LogLevel.Success, $"Dine recorded DineId: {dine.Id}, OriPrice: {dine.OriPrice}, Price: {dine.Price}, IsOnline: {dine.IsOnline}, Via {via}");
			NewDineInformTcpClient.SendNewDineInfrom(CurrHotel.Id, dine.Id, false);
		}

		public async Task<JsonResult> PrintCompleted(string dineId) {
			await OrderManager.PrintCompleted(dineId);
			await HotelManager.RecordLog(HotelDAO.Models.Log.LogLevel.Success, $"PrintCompleted DineId: {dineId}");
			return Json(new JsonSuccess());
		}
	}
}