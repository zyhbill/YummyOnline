using HotelDAO;
using HotelDAO.Models;
using Newtonsoft.Json;
using OrderSystem.Models;
using Protocal;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using YummyOnlineDAO;
using YummyOnlineDAO.Identity;
using YummyOnlineDAO.Models;

namespace OrderSystem.Controllers {
	public class PaymentController : BaseOrderSystemController {
		private StaffManager _staffManager;
		public StaffManager StaffManager {
			get {
				if(_staffManager == null) {
					_staffManager = new StaffManager();
				}
				return _staffManager;
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

		/// <summary>
		/// 支付完成页面，无关数据记录
		/// </summary>
		/// <returns></returns>
		public ActionResult Complete(bool? succeeded, string dineId) {
			ViewBag.Succeeded = succeeded == null ? true : succeeded;
			ViewBag.DineId = dineId;
			return View();
		}

		/// <summary>
		/// 普通用户支付
		/// </summary>
		/// <param name="cart"></param>
		/// <returns></returns>
		[RequireHotel]
		public async Task<JsonResult> Pay(Cart cart) {
			CartAddition addition = new CartAddition();

			// 新建或获取用户Id
			User user = await createOrGetUser(User.Identity.GetUserId());
			if(user == null) {
				return Json(new JsonError("创建匿名用户失败"));
			}
			SigninManager.Signin(user, true);
			addition.UserId = user.Id;

			// 创建新订单
			FunctionResult result = await OrderManager.CreateDine(cart, addition);
			if(!result.Succeeded) {
				await HotelManager.RecordLog(HotelDAO.Models.Log.LogLevel.Error, result.Detail);
				return Json(new JsonError(result.Message));
			}

			Dine dine = ((Dine)result.Data);

			await newDineInform(dine, "OrderSystem");

			PayKind payKind = await HotelManager.GetPayKind(cart.PayKindId);
			string redirectUrl = null;

			if(payKind.Type == PayKindType.Online) {
				DinePaidDetail paidDetail = dine.DinePaidDetails.FirstOrDefault(p => p.PayKind.Id == payKind.Id);
				if(Math.Abs((double)(paidDetail.Price - 0)) < 0.01) {
					redirectUrl = $"{payKind.CompleteUrl}?Succeeded={true}&DineId={dine.Id}";
					await onlinePayCompleted(dine.Id, null);
				}
				else {
					redirectUrl = await getOnlineRedirectUrl(dine.Id);
				}
			}
			else {
				await requestPrintDine(dine.Id);
				redirectUrl = $"{payKind.CompleteUrl}?Succeeded={true}&DineId={dine.Id}";
			}

			return Json(new JsonSuccess(redirectUrl));
		}

		/// <summary>
		/// 收银员台支付
		/// </summary>
		/// <param name="cart"></param>
		/// <param name="cartAddition"></param>
		/// <returns></returns>
		public async Task<JsonResult> ManagerPay(Cart cart, ManagerCartAddition cartAddition) {
			if(!await verifyToken(cartAddition.Token)) {
				return Json(new JsonError("身份验证失败"));
			}

			CurrHotel = await YummyOnlineManager.GetHotelById(cartAddition.HotelId);

			cart.PayKindId = await new HotelManagerForWaiter(CurrHotel.ConnectionString).GetOtherPayKindId();
			CartAddition addition = new CartAddition {
				WaiterId = cartAddition.WaiterId,
				Discount = cartAddition.Discount,
				DiscountName = cartAddition.DiscountName,
				GiftMenus = cartAddition.GiftMenus
			};

			User user = await createOrGetUser(cartAddition.UserId);
			if(user == null) {
				return Json(new JsonError("创建匿名用户失败"));
			}
			addition.UserId = user.Id;

			// 创建新订单
			FunctionResult result = await OrderManager.CreateDine(cart, addition);
			if(!result.Succeeded) {
				await HotelManager.RecordLog(HotelDAO.Models.Log.LogLevel.Error, result.Detail);
				return Json(new JsonError(result.Message));
			}

			Dine dine = ((Dine)result.Data);

			await newDineInform(dine, "Manager");

			return Json(new JsonSuccess());
		}

		/// <summary>
		/// 服务员支付
		/// </summary>
		/// <param name="cart"></param>
		/// <param name="cartAddition"></param>
		/// <returns></returns>
		public async Task<JsonResult> WaiterPay(Cart cart, WaiterCartAddition cartAddition, string waiterId, string token) {
			if(!await verifyToken(token)) {
				return Json(new JsonError("身份验证失败"));
			}

			FunctionResult result = await waiterPay(cart, cartAddition, waiterId);

			if(!result.Succeeded) {
				return Json(new JsonError(result.Message));
			}

			Dine dine = ((Dine)result.Data);

			await newDineInform(dine, "Waiter");

			return Json(new JsonSuccess { Data = dine.Id });
		}

		/// <summary>
		/// 服务员支付完成，记录支付详情
		/// </summary>
		/// <param name="paidDetails"></param>
		/// <returns></returns>
		public async Task<JsonResult> WaiterPayCompleted(WaiterPaidDetails paidDetails, string waiterId, string token) {
			if(!await verifyToken(token)) {
				return Json(new JsonError("身份验证失败"));
			}

			var staff = await StaffManager.FindStaffById(waiterId);
			if(staff == null) {
				return Json(new JsonError("未找到服务员"));
			}

			CurrHotel = await YummyOnlineManager.GetHotelById(staff.HotelId);

			bool succeeded = await OrderManager.OfflinePayCompleted(paidDetails);
			if(!succeeded) {
				return Json(new JsonError("支付金额与应付金额不匹配"));
			}

			NewDineInformTcpClient.SendNewDineInfrom(CurrHotel.Id, paidDetails.DineId, true);

			return Json(new JsonSuccess { Data = paidDetails.DineId });
		}
		/// <summary>
		/// 服务员支付并且带所有支付详情
		/// </summary>
		/// <param name="cart"></param>
		/// <param name="cartAddition"></param>
		/// <param name="paidDetails"></param>
		/// <returns></returns>
		public async Task<JsonResult> WaiterPayWithPaidDetails(Cart cart, WaiterCartAddition cartAddition, WaiterPaidDetails paidDetails, string waiterId, string token) {
			if(!await verifyToken(token)) {
				return Json(new JsonError("身份验证失败"));
			}

			FunctionResult result = await waiterPay(cart, cartAddition, waiterId);

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



		/// <summary>
		/// 重新支付
		/// </summary>
		/// <param name="dineId"></param>
		/// <returns></returns>
		public async Task<JsonResult> PayAgain(string dineId) {
			return Json(new JsonSuccess(await getOnlineRedirectUrl(dineId)));
		}

		/// <summary>
		/// 支付完成异步通知
		/// </summary>
		public async Task<JsonResult> OnlineNotify(string encryptedInfo) {
			string decryptedInfo = Cryptography.DesCryptography.DesDecrypt(encryptedInfo);
			NetworkNotifyViewModels model = null;

			try {
				model = JsonConvert.DeserializeObject<NetworkNotifyViewModels>(decryptedInfo);
			}
			catch {
				return Json(new JsonError());
			}

			CurrHotel = await YummyOnlineManager.GetHotelById(model.HotelId);

			await HotelManager.RecordLog(HotelDAO.Models.Log.LogLevel.Info, $"Notified DineId: {model.DineId}");
			await onlinePayCompleted(model.DineId, model.RecordId);

			return Json(new JsonSuccess());
		}

		/// <summary>
		/// 打印完成
		/// </summary>
		/// <param name="hotelId"></param>
		/// <param name="dineId"></param>
		/// <returns></returns>
		public async Task PrintCompleted(int hotelId, string dineId) {
			CurrHotel = await YummyOnlineManager.GetHotelById(hotelId);
			string connStr = await YummyOnlineManager.GetHotelConnectionStringById(hotelId);
			OrderManager orderManager = new OrderManager(connStr);
			await orderManager.PrintCompleted(dineId);
			await new HotelManager(connStr).RecordLog(HotelDAO.Models.Log.LogLevel.Success, $"PrintCompleted DineId: {dineId}");
		}

		private async Task<User> createOrGetUser(string userId) {
			User user = await UserManager.FindByIdAsync(userId);
			if(user == null) {
				user = await UserManager.CreateVoidUserAsync();
				if(user == null) {
					return null;
				}
				await YummyOnlineManager.RecordLog(YummyOnlineDAO.Models.Log.LogProgram.Identity, YummyOnlineDAO.Models.Log.LogLevel.Success, $"Anonymous User Created {user.Id}");
				await UserManager.AddToRoleAsync(user.Id, Role.Nemo);
			}
			return user;
		}
		private async Task newDineInform(Dine dine, string via) {
			await HotelManager.RecordLog(HotelDAO.Models.Log.LogLevel.Success, $"Dine recorded DineId: {dine.Id}, OriPrice: {dine.OriPrice}, Price: {dine.Price}, IsOnline: {dine.IsOnline}, Via {via}");
			NewDineInformTcpClient.SendNewDineInfrom(CurrHotel.Id, dine.Id, false);
		}
		private async Task<string> getOnlineRedirectUrl(string dineId) {
			HotelConfig hotelConfig = await HotelManager.GetHotelConfig();
			DinePaidDetail dinePaidDetail = await HotelManager.GetDineOnlinePaidDetail(dineId);
			if(dinePaidDetail == null)
				return null;

			StringBuilder redirectUrl = new StringBuilder();
			redirectUrl.Append($"{dinePaidDetail.PayKind.RedirectUrl}?");
			string priceCrypted = Cryptography.DesCryptography.DesEncrypt(dinePaidDetail.Price.ToString());
			redirectUrl.Append($"HotelId={hotelConfig.Id}&DineId={dineId}&Price={Server.UrlEncode(priceCrypted)}&");
			redirectUrl.Append($"NotifyUrl={Server.UrlEncode(dinePaidDetail.PayKind.NotifyUrl)}&");
			redirectUrl.Append($"CompleteUrl={Server.UrlEncode(dinePaidDetail.PayKind.CompleteUrl)}");
			return redirectUrl.ToString();
		}
		private async Task onlinePayCompleted(string dineId, string recordId) {
			bool isPaid = await new HotelManagerForWaiter(CurrHotel.ConnectionString).IsDinePaid(dineId);
			if(isPaid) {
				await HotelManager.RecordLog(HotelDAO.Models.Log.LogLevel.Warning, $"DineId: {dineId} Has Been Paid");
				return;
			}
			await OrderManager.OnlinePayCompleted(dineId, recordId);
			NewDineInformTcpClient.SendNewDineInfrom(CurrHotel.Id, dineId, true);
			await requestPrintDine(dineId);
		}

		private async Task<bool> verifyToken(string token) {
			SystemConfig system = await YummyOnlineManager.GetSystemConfig();
			if(system.Token != token) {
				return false;
			}
			return true;
		}

		private async Task<FunctionResult> waiterPay(Cart cart, WaiterCartAddition cartAddition, string waiterId) {
			var staff = await StaffManager.FindStaffById(waiterId);
			if(staff == null) {
				return new FunctionResult(false, "未找到服务员");
			}

			CurrHotel = await YummyOnlineManager.GetHotelById(staff.HotelId);

			HotelManagerForWaiter hotelManager = new HotelManagerForWaiter(CurrHotel.ConnectionString);
			cart.PayKindId = await hotelManager.GetOtherPayKindId();
			CartAddition addition = new CartAddition {
				WaiterId = staff.Id,
				Discount = cartAddition.Discount,
				DiscountName = cartAddition.DiscountName
			};

			User user = await createOrGetUser(cartAddition.UserId);
			if(user == null) {
				return new FunctionResult(false, "创建匿名用户失败");
			}
			addition.UserId = user.Id;

			// 创建新订单
			FunctionResult result = await OrderManager.CreateDine(cart, addition);
			if(result.Succeeded) {
				await hotelManager.AddStaffDine(staff.Id, ((Dine)result.Data).Price);
			}
			else {
				await HotelManager.RecordLog(HotelDAO.Models.Log.LogLevel.Error, result.Detail);
			}
			return result;
		}

		private async Task requestPrintDine(string dineId) {
			HotelConfig config = await HotelManager.GetHotelConfig();
			if(config.HasAutoPrinter) {
				NewDineInformTcpClient.SendRequestPrintDine(CurrHotel.Id, dineId);
			}
		}
	}
}