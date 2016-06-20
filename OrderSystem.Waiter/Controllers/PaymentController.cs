using HotelDAO;
using HotelDAO.Models;
using OrderSystem.Models;
using Protocal;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Mvc;
using Utility;
using YummyOnlineDAO.Identity;
using YummyOnlineDAO.Models;

namespace OrderSystem.Waiter.Controllers {
	[Authorize(Roles = nameof(Schema.SubmitWaiterPay))]
	[HotelAvailable]
	public partial class PaymentController : BaseWaiterController {
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
			FunctionResult result = await waiterPay(cart, cartAddition, "Waiter");
			if(!result.Succeeded) {
				return Json(new JsonError(result.Message));
			}

			Dine dine = ((Dine)result.Data);
			await newDineInform(dine, "Waiter");
			await requestPrintDine(dine.Id);
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
			FunctionResult result = await waiterPay(cart, cartAddition, "WaiterWithPaidDetails");
			if(!result.Succeeded) {
				return Json(new JsonError(result.Message));
			}

			Dine dine = ((Dine)result.Data);
			await newDineInform(dine, "WaiterWithPaidDetail");
			await requestPrintDine(dine.Id);
			paidDetails.DineId = dine.Id;

			bool succeeded = await OrderManager.OfflinePayCompleted(paidDetails);
			if(!succeeded) {
				return Json(new JsonError("支付金额与应付金额不匹配"));
			}

			NewDineInformTcpClient.SendNewDineInfrom(CurrHotel.Id, paidDetails.DineId, true);
			return Json(new JsonSuccess { Data = paidDetails.DineId });
		}

		public async Task<JsonResult> PrintCompleted(string dineId) {
			await OrderManager.PrintCompleted(dineId);
			await HotelManager.RecordLog(HotelDAO.Models.Log.LogLevel.Success, $"PrintCompleted DineId: {dineId}");
			return Json(new JsonSuccess());
		}
	}

	// PaymentController辅助函数
	public partial class PaymentController {
		private async Task<FunctionResult> waiterPay(Cart cart, WaiterCartAddition cartAddition, string via) {
			HotelManager hotelManager = new HotelManager(CurrHotel.ConnectionString);
			cart.PayKindId = await hotelManager.GetOtherPayKindId();
			CartAddition addition = new CartAddition {
				WaiterId = User.Identity.GetUserId(),
				Discount = cartAddition.Discount,
				DiscountName = cartAddition.DiscountName
			};

			User user = await UserManager.FindByIdAsync(cartAddition.UserId);
			addition.UserId = user?.Id;

			// 创建新订单
			FunctionResult result = await OrderManager.CreateDine(cart, addition);
			if(!result.Succeeded) {
				await HotelManager.RecordLog(HotelDAO.Models.Log.LogLevel.Error, $"{result.Detail}", HttpPost.GetPostData(Request));
			}

			return result;
		}

		private async Task newDineInform(Dine dine, string via) {
			await HotelManager.RecordLog(HotelDAO.Models.Log.LogLevel.Success, $"Dine recorded DineId: {dine.Id}, Price: {dine.Price}, IsOnline: {dine.IsOnline}, Via {via}",
				HttpPost.GetPostData(Request));
			NewDineInformTcpClient.SendNewDineInfrom(CurrHotel.Id, dine.Id, false);
		}
		/// <summary>
		/// 请求打印订单
		/// </summary>
		/// <param name="dineId">订单号</param>
		/// <returns></returns>
		private async Task requestPrintDine(string dineId) {
			HotelConfig config = await new HotelManager(CurrHotel.ConnectionString).GetHotelConfig();
			if(config.HasAutoPrinter) {
				NewDineInformTcpClient.SendRequestPrintDine(CurrHotel.Id, dineId, new List<PrintType> { PrintType.Recipt, PrintType.ServeOrder, PrintType.KitchenOrder });
			}
		}
	}
}