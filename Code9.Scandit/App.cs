using System;
using Xamarin.Forms;

namespace Code9.Scandit
{
	public class App
	{
		public static Page GetMainPage ()
		{	
			Button scanButton = new Button() {
				HorizontalOptions = LayoutOptions.Center,
				Text = "Tap to Scan"
			};

			Label message = new Label () {
				XAlign = TextAlignment.Center
			};

			IScanner scanner = DependencyService.Get<IScanner> ();

			//Configure scanner. Please get your own Scandit key!
			scanner.Configure ("xbENFiLaEeSCn/KdTNpva0lad8WlMjCpK89rxosdty4", "Cancel");

			scanButton.Clicked += async (object sender, EventArgs e) => {

				message.Text = string.Empty;

				ScanResult result =  await scanner.ScanAsync();

				if (result.Success) {
					message.Text = string.Format("Success. Code = {0}", result.Code);
				}
				else {
					message.Text = "Failed";
				}
			};

			return new ContentPage { 
				Content = new StackLayout() {
					Padding = 5,
					Spacing = 10,
					VerticalOptions = LayoutOptions.CenterAndExpand,
					Children = {
						scanButton,
						message
					}
				}
			};
		}
	}
}

