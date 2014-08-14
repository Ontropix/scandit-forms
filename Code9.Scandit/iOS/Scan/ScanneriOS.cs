using System;
using Xamarin.Forms;
using ScanditSDK;
using MonoTouch.UIKit;
using MonoTouch.Foundation;
using System.Threading.Tasks;

[assembly: Dependency(typeof(Code9.iOS.ScanneriOS))]
namespace Code9.iOS
{

	public class ScanneriOS: IScanner
	{

		private string cancelText;
		private string licenseKey;

		public System.Threading.Tasks.Task<ScanResult> ScanAsync ()
		{
			UIViewController root = UIApplication.SharedApplication.KeyWindow.RootViewController;

			TaskCompletionSource<ScanResult> tcs = new TaskCompletionSource<ScanResult> ();

			var picker = new SIBarcodePicker (licenseKey);
			picker.OverlayController.ShowToolBar (true);
			picker.OverlayController.SetToolBarButtonCaption (cancelText);

			picker.OverlayController.Delegate = new PickerDelegate () { 
				Picker = picker, 
				Completion = tcs 
			};

			root.PresentViewController (picker, true, null);

			picker.StartScanning ();

			return tcs.Task;
		}

		public	void Configure(string licenseKey, string cancelText){
			this.licenseKey = licenseKey;
			this.cancelText = cancelText;
		}


		private class PickerDelegate : SIOverlayControllerDelegate
		{
			public SIBarcodePicker Picker {get;set;}
			public 	TaskCompletionSource<ScanResult> Completion { get; set; }

			public override void DidScanBarcode (SIOverlayController overlayController, NSDictionary barcode) {
				// perform actions after a barcode was scanned
				Console.WriteLine ("barcode scanned: {0}, '{1}'", barcode["symbology"], barcode["barcode"]);
				Completion.SetResult (new ScanResult() {
					Success = true,
					Code = barcode ["barcode"].ToString()
				});
				Picker.StopScanning ();
				Picker.DismissViewController (true, null);
			}

			public override void DidCancel (SIOverlayController overlayController, NSDictionary status) {
				Completion.SetResult (new ScanResult() {Success = false});
				Picker.StopScanning ();
				Picker.DismissViewController (true, null);
			}
				

			public override void DidManualSearch (SIOverlayController overlayController, string text)
			{
				//Do nothing
			}

		}
			
	}
}

