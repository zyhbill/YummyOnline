using HotelDAO.Models;
using Newtonsoft.Json;
using OrderSystem.Models;
using Protocal;
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Mvc;
using YummyOnlineDAO.Models;
using Utility;

namespace OrderSystem.Waiter.Controllers {
	[Authorize(Roles = nameof(Schema.SubmitWaiterPay))]
	[HotelAvailable]
	public class PaymentController : BaseWaiterController {
		public async Task<JsonResult> WaiterPay(Cart cart, WaiterCartAddition cartAddition) {
			SystemConfig config = await YummyOnlineManager.GetSystemConfig();

			string responseContent = await HttpPost.PostAsync($"{config.OrderSystemUrl}/Payment/{nameof(WaiterPay)}", new {
				Cart = cart,
				CartAddition = cartAddition,
				WaiterId = User.Identity.Name,
				Token = config.Token
			});

			if(string.IsNullOrEmpty(responseContent)) {
				return Json(new JsonError("网络错误"));
			}
			return Json(JsonConvert.DeserializeObject(responseContent));
		}
		public async Task<JsonResult> WaiterPayCompleted(WaiterPaidDetails paidDetails) {
			SystemConfig config = await YummyOnlineManager.GetSystemConfig();

			string responseContent = await HttpPost.PostAsync($"{config.OrderSystemUrl}/Payment/{nameof(WaiterPayCompleted)}", new {
				PaidDetails = paidDetails,
				WaiterId = User.Identity.Name,
				Token = config.Token
			});

			if(string.IsNullOrEmpty(responseContent)) {
				return Json(new JsonError("网络错误"));
			}
			return Json(JsonConvert.DeserializeObject(responseContent));
		}
		public async Task<JsonResult> WaiterPayWithPaidDetails(Cart cart, WaiterCartAddition cartAddition, WaiterPaidDetails paidDetails) {
			SystemConfig config = await YummyOnlineManager.GetSystemConfig();

			string responseContent = await HttpPost.PostAsync($"{config.OrderSystemUrl}/Payment/{nameof(WaiterPayWithPaidDetails)}", new {
				Cart = cart,
				CartAddition = cartAddition,
				PaidDetails = paidDetails,
				WaiterId = User.Identity.Name,
				Token = config.Token
			});

			if(string.IsNullOrEmpty(responseContent)) {
				return Json(new JsonError("网络错误"));
			}
			return Json(JsonConvert.DeserializeObject(responseContent));
		}
	}
}