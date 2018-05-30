using NUnit.Framework;
using System.IO;
using System.Threading.Tasks;

namespace HttpClientRepro.Tests
{
	[TestFixture]
    public class HttpTest
    {
		[Test]
		public async Task ItWorks()
		{
			var path = Path.GetTempFileName ();
			try {
				var http = Http.GetInstance ();
				using (var stream = File.Create (path)) {
					var bytes = new byte [1024 * 1024];
					for (int i = 0; i < 32; i++) {
						stream.Write (bytes, 0, bytes.Length);
					}
				}
				var result = await http.RankingUploadAsync (path);
			} finally {
				File.Delete (path);
			}
		}
    }
}
