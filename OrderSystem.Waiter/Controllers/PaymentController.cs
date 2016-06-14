using HotelDAO;
using HotelDAO.Models;
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
			HotelManagerForWaiter hotelManager = new HotelManagerForWaiter(CurrHotel.ConnectionString);
			cart.PayKindId = await hotelManager.GetOtherPayKindId();
			CartAddition addition = new CartAddition {
				WaiterId = User.Identity.GetUserId(),
				Discount = cartAddition.Discount,
				DiscountName = cartAddition.DiscountName
			};

			User user = await createOrGetUser(cartAddition.UserId, via);
			if(user == null) {
				return new FunctionResult(false, "创建匿名用户失败");
			}
			addition.UserId = user.Id;

			// 创建新订单
			FunctionResult result = await OrderManager.CreateDine(cart, addition);
			if(!result.Succeeded) {
				if(await UserManager.IsInRoleAsync(user.Id, Role.Nemo)) {
					await UserManager.DeleteAsync(user);
					await YummyOnlineManager.RecordLog(YummyOnlineDAO.Models.Log.LogProgram.Identity, YummyOnlineDAO.Models.Log.LogLevel.Warning, $"Anonymous User Deleted {user.Id}, Via {via}");
				}
				await HotelManager.RecordLog(HotelDAO.Models.Log.LogLevel.Error, $"{result.Detail}", HttpPost.GetPostData(Request));
			}

			return result;
		}

		/// <summary>
		/// 创建或获取用户
		/// </summary>
		/// <param name="userId">用户id</param>
		/// <returns>当前或新建的用户</returns>
		private async Task<User> createOrGetUser(string userId, string via) {
			User user = await UserManager.FindByIdAsync(userId);
			if(user == null) {
				user = await UserManager.CreateVoidUserAsync();
				if(user == null) {
					return null;
				}
				await YummyOnlineManager.RecordLog(YummyOnlineDAO.Models.Log.LogProgram.Identity, YummyOnlineDAO.Models.Log.LogLevel.Success, $"Anonymous User Created {user.Id}, Via {via}");
				await UserManager.AddToRoleAsync(user.Id, Role.Nemo);
			}
			return user;
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
		//private async Task requestPrintDine(string dineId) {
		//	HotelConfig config = await new HotelManager(CurrHotel.ConnectionString).GetHotelConfig();
		//	if(config.HasAutoPrinter) {
		//		NewDineInformTcpClient.SendRequestPrintDine(CurrHotel.Id, dineId);
		//	}
		//}
	}
}