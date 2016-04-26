using OrderSystem.Models;
using Protocal;
using System.Web;
using System.Web.Mvc;
using System.Net;
using System.Net.Http;
using System;
using System.Threading.Tasks;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using HotelDAO.Models;

namespace OrderSystem.Waiter.Controllers {
	[Authorize(Roles = nameof(Schema.SubmitWaiterPay))]
	public class PaymentController : BaseWaiterController {
		public async Task<JsonResult> WaiterPay(Cart cart, WaiterCartAddition cartAddition) {
			string url = (await YummyOnlineManager.GetSystemConfig()).OrderSystemUrl;

			HttpClientHandler handler = new HttpClientHandler();
			handler.CookieContainer = new CookieContainer();

			foreach(var cookieKey in Request.Cookies.AllKeys) {
				handler.CookieContainer.Add(new Uri(url),
					new Cookie(cookieKey, Request.Cookies[cookieKey].Value));
			}

			cartAddition.HotelId = CurrHotel.Id;
			string contentStr = JsonConvert.SerializeObject(new {
				Cart = cart,
				CartAddition = cartAddition,
			});
			StringContent content = new StringContent(contentStr);
			content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");

			HttpClient httpClient = new HttpClient(handler);
			try {
				HttpResponseMessage response = await httpClient.PostAsync($"{url}/Payment/WaiterPay", content);
				if(response != null) {
					if(response.StatusCode == HttpStatusCode.OK) {
						string json = await response.Content.ReadAsStringAsync();
						return Json(JsonConvert.DeserializeObject(json));
					}
				}
			}
			catch { }
			
			return Json(new JsonError("网络错误"));
		}
	}
}