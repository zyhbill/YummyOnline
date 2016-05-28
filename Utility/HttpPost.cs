using Newtonsoft.Json;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace Utility {
	public static class HttpPost {
		public static async Task<string> PostAsync(string url, object postData, string contentType = "application/json") {
			try {
				HttpClient client = new HttpClient();
				StringContent content = new StringContent(JsonConvert.SerializeObject(postData));
				content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(contentType);
				content.Headers.Add("X-Requested-With", "XMLHttpRequest");
				HttpResponseMessage response = await client.PostAsync(url, content);
				if(response != null) {
					if(response.StatusCode == HttpStatusCode.OK) {
						return await response.Content.ReadAsStringAsync();
					}
				}
			}
			catch { }
			return null;
		}
	}
}
