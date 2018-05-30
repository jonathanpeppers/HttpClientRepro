using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Text;
using System.IO;
using System.Net.Http.Headers;
using Newtonsoft.Json;

namespace HttpClientRepro
{
	//Ported from: https://github.com/xamarin/xamarin-android/issues/1742#issuecomment-392952341
	public class Http
	{
		static Http http = new Http ();

		readonly HttpClient _httpClient = new HttpClient ();

		const string BASE_URL = "http://httpbin.org/anything/";
		const string RANKINGUPLOAD_URL = "ranking/upload";
		const bool FAST = true;

		private Http () { }

		public static Http GetInstance ()
		{
			return http;
		}

		public async Task<RankingUploadResponse> RankingUploadAsync (string pathVideo)
		{

			MultipartFormDataContent multipartFormDataContent = new MultipartFormDataContent ();
			multipartFormDataContent.Add (new StringContent ("1234"), "PRSCAU");
			multipartFormDataContent.Add (new StringContent ("5678"), "PRSCAM");

			multipartFormDataContent = prepareForm (multipartFormDataContent, pathVideo);

			_httpClient.DefaultRequestHeaders.Authorization = authenticationHeaderValue ("bearer", "myToken");

			/* This is the row that thrown exception */
			HttpResponseMessage httpResponseMessage = await _httpClient.PostAsync (createUrl (RANKINGUPLOAD_URL), multipartFormDataContent);
			return await readStreamAndDeserialize<RankingUploadResponse> (httpResponseMessage);
		}

		MultipartFormDataContent prepareForm (MultipartFormDataContent multipartFormDataContent, string path)
		{
			multipartFormDataContent.Add (new StreamContent (File.OpenRead (path)), "Files", Path.GetFileName (path));
			
			return multipartFormDataContent;
		}

		AuthenticationHeaderValue authenticationHeaderValue (String type, String token)
		{
			return new AuthenticationHeaderValue (type, token);
		}

		async Task<T> readStreamAndDeserialize<T> (HttpResponseMessage response) where T : new()
		{
			response.EnsureSuccessStatusCode ();

			using (var stream = await response.Content.ReadAsStreamAsync ())
			using (var reader = new StreamReader (stream)) {
				if (FAST) {
					using (var json = new JsonTextReader (reader)) {
						return deserialize<T> (json);
					}
				} else {
					string text = reader.ReadToEnd ();
					Console.WriteLine ("RECEIVED: " + text);
					return JsonConvert.DeserializeObject<T> (text);
				}
			}
		}

		T deserialize<T> (JsonTextReader json) where T : new()
		{
			return new JsonSerializer ().Deserialize<T> (json);
		}

		string createUrl (string url)
		{
			return BASE_URL + url;
		}
	}

	public class RankingUploadResponse
	{

	}
}
