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

namespace OrderSystem.Waiter.Controllers {
	[Authorize(Roles = nameof(Schema.SubmitWaiterPay))]
	public class PaymentController : BaseWaiterController {
		public async Task<JsonResult> WaiterPay(Cart cart, WaiterCartAddition cartAddition) {
			SystemConfig config = await YummyOnlineManager.GetSystemConfig();

			string responseContent = await postAsync($"{config.OrderSystemUrl}/Payment/{nameof(WaiterPay)}", new {
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

			string responseContent = await postAsync($"{config.OrderSystemUrl}/Payment/{nameof(WaiterPayCompleted)}", new {
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

			string responseContent = await postAsync($"{config.OrderSystemUrl}/Payment/{nameof(WaiterPayWithPaidDetails)}", new {
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

		private async Task<string> postAsync(string url, object postData, string contentType = "application/json") {
			try {
				HttpClient client = new HttpClient();
				StringContent content = new StringContent(JsonConvert.SerializeObject(postData));
				content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(contentType);
				HttpResponseMessage response = await client.PostAsync(url, content);
				if(response != null) {
					if(response.StatusCode == HttpStatusCode.OK) {
						return await response.Content.ReadAsStringAsync();
					}
					else {
						await YummyOnlineManager.RecordLog(YummyOnlineDAO.Models.Log.LogProgram.OrderSystem_Waiter,
							YummyOnlineDAO.Models.Log.LogLevel.Error,
							$"StatusCode: {response.StatusCode}");
					}
				}
			}
			catch(Exception e) {
				await YummyOnlineManager.RecordLog(YummyOnlineDAO.Models.Log.LogProgram.OrderSystem_Waiter,
							YummyOnlineDAO.Models.Log.LogLevel.Error,
							$"{e.Message}");
			}
			return null;
		}
	}
}