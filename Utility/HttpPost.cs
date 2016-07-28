using Newtonsoft.Json;
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;

namespace Utility {
	public static class HttpPost {
		private static HttpClient client = new HttpClient();

		public static async Task<string> PostAsync(string url, object postData, string contentType = "application/json") {
			try {
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
			catch(Exception e) {
				System.Diagnostics.Debug.WriteLine(e);
			}
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
