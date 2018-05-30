using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace HttpClientRepro
{
	public partial class MainPage : ContentPage
	{
		public MainPage()
		{
			InitializeComponent();
		}

		private async void Button_Clicked (object sender, EventArgs e)
		{
			((Button)sender).IsVisible = false;

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
				await DisplayAlert ("Success", "It worked!", "Ok");
			} catch (Exception exc) {
				await DisplayAlert ("Failure", exc.Message, "Ok");
			} finally {
				File.Delete (path);

				((Button)sender).IsVisible = true;
			}
		}
	}
}
