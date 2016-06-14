using Newtonsoft.Json;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;

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
		public static string GetPostData(HttpRequestBase request) {
			request.InputStream.Seek(0, SeekOrigin.Begin);
			return new StreamReader(request.InputStream).ReadToEnd();
		}
		public static string GetPostData(HttpRequest request) {
			request.InputStream.Seek(0, SeekOrigin.Begin);
			return new StreamReader(request.InputStream).ReadToEnd();
		}
	}
}
