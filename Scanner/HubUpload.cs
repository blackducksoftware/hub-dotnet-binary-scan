using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Text;
using System.Threading.Tasks;

namespace Blackduck.Hub
{
	class HubUpload
	{
		private const string UPLOAD_URI = "api/v1/scans/upload";
		private const string AUTH_URI = "j_spring_security_check";

		public static void UploadScan(string baseUrl, string username, string password, ScannerJsonBuilder scanResult)
		{
			IEnumerable<Cookie> authCookies = authenticate(baseUrl, username, password);
			//Prepare the cookies
			//Ensure no trailing slash
			var cookieContainer = new CookieContainer();
			var cookieBaseUri = new Uri(baseUrl.EndsWith("/") ? baseUrl.Substring(0, baseUrl.Length - 1) : baseUrl);

			foreach (Cookie authCookie in authCookies)
				cookieContainer.Add(cookieBaseUri, authCookie);

			string requestUrl = $"{(baseUrl.EndsWith("/") ? baseUrl : baseUrl + "/")}{UPLOAD_URI}";


			var clientHandler = new HttpClientHandler() { CookieContainer = cookieContainer };
			var httpClient = new HttpClient(clientHandler);

			var content = new MultipartFormDataContent();


			using (var ms = new MemoryStream())
			using (var writer = new StreamWriter(ms))
			{
				scanResult.Write(writer);
				var streamContent = new ByteArrayContent(ms.ToArray());
				streamContent.Headers.ContentType = System.Net.Http.Headers.MediaTypeHeaderValue.Parse("application/json");
				content.Add(streamContent, "file", "scan.json");

				var response = httpClient.PostAsync(requestUrl, content);
				Task.WaitAll(response);

				if (response.Result.StatusCode != HttpStatusCode.OK && response.Result.StatusCode != HttpStatusCode.Created)
				{
					throw new Exception($"Unable to upload to hub. Status code: {response.Result.StatusCode}. {response.Result.ReasonPhrase}");
				}
			}

		}

		/// <summary>
		/// Authenticates with the hub and returns the cookies to set.
		/// </summary>
		/// <param name="baseUrl"></param>
		/// <param name="username"></param>
		/// <param name="password"></param>
		/// <returns></returns>
		private static IEnumerable<Cookie> authenticate(string baseUrl, string username, string password)
		{
			string requestUrl = $"{(baseUrl.EndsWith("/") ? baseUrl : baseUrl + "/")}{AUTH_URI}";

			StringBuilder formData = new StringBuilder();
			formData.Append("j_username=" + HttpUtility.UrlEncode(username));
			formData.Append("&j_password=" + HttpUtility.UrlEncode(password));
			byte[] requestData = Encoding.ASCII.GetBytes(formData.ToString());

			HttpContent content = new ByteArrayContent(requestData);
			content.Headers.ContentType = System.Net.Http.Headers.MediaTypeHeaderValue.Parse("application/x-www-form-urlencoded");

			var requestTask = new HttpClient().PostAsync(requestUrl, content);
			Task.WaitAll(requestTask);

			var response = requestTask.Result;


			if (response.StatusCode != HttpStatusCode.NoContent && response.StatusCode != HttpStatusCode.OK)
			{
				throw new Exception($"Unable to authenticate into {baseUrl}. {response.ReasonPhrase} ({response.StatusCode})");
			}

			IEnumerable<string> cookies;
			if (!response.Headers.TryGetValues("Set-Cookie", out cookies))
				cookies = Enumerable.Empty<string>();

			var cleansedCookies = new List<Cookie>();
			foreach (string cookie in cookies.Where(c => !string.IsNullOrEmpty(c)))
			{
				string cleansedCookie = string.Join(";", cookie.Split(';').Where(c => !c.StartsWith("Path=")));
				string[] cookieParts = cleansedCookie.Split('=');
				cleansedCookies.Add(new Cookie(cookieParts[0], cookieParts[1]));
			}
			return cleansedCookies;
		}
	}
}
