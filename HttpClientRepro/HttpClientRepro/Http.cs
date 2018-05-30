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
		const string CONTEXT = "";

		const string RANKINGUPLOAD_URL = "ranking/upload";

		const int TIMEOUT = 200;
		const bool FAST = true;


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
			HttpResponseMessage httpResponseMessage = await _httpClient.PostAsync (createUrl (RANKINGUPLOAD_URL, false), multipartFormDataContent);
			return await readStreamAndDeserialize<RankingUploadResponse> (httpResponseMessage);
		}

		MultipartFormDataContent prepareForm (MultipartFormDataContent multipartFormDataContent, string path)
		{
			byte [] imagebytearraystring = ImageFileToByteArray (path);
			string name = Path.GetFileName (path);
			string fileName = name;
			multipartFormDataContent.Add (new ByteArrayContent (imagebytearraystring, 0, imagebytearraystring.Length), "Files", fileName);

			return multipartFormDataContent;

		}

		private byte [] ImageFileToByteArray (string fullFilePath)
		{
			FileStream fs = File.OpenRead (fullFilePath);
			byte [] bytes = new byte [fs.Length];
			fs.Read (bytes, 0, Convert.ToInt32 (fs.Length));
			fs.Close ();
			return bytes;
		}

		async Task<String> readStream (HttpResponseMessage response)
		{
			if (response.IsSuccessStatusCode) {
				using (var stream = await response.Content.ReadAsStreamAsync ())
				using (var reader = new StreamReader (stream)) {
					string text = reader.ReadToEnd ();
					Console.WriteLine ("RECEIVED: " + text);
					return text;

				}
			}

			return null;
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

		Uri createUri (String url, bool addContext)
		{
			return new Uri (createUrl (url, addContext));
		}

		string createUrl (String url, bool addContext)
		{
			return String.Format ("{0}{1}{2}", BASE_URL, addContext ? CONTEXT : "", url);
		}

		StringContent createStringContent (object oggetto)
		{
			return new StringContent (JsonConvert.SerializeObject (oggetto), Encoding.UTF8, "application/json");
		}

	}

	public class RankingUploadResponse
	{

	}
}
