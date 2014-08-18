using System;
using Android.Views;
using Scandit;
using Scandit.Interfaces;
using Xamarin.Forms;
using Android.OS;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using System.Threading;
using Android.Content.PM;
using Android.Support.V4.App;

[assembly: Dependency(typeof(Code9.Android.ScannerDroid))]
namespace Code9.Android
{
	public class ScannerDroid: IScanner
	{
		private string cancelText;
		private string licenseKey;

		public	void Configure(string licenseKey, string cancelText){
			this.licenseKey = licenseKey;
			this.cancelText = cancelText;
		}

		ScanResult scanResult = null;
		ManualResetEvent waitScanResetEvent = null;

		public System.Threading.Tasks.Task<ScanResult> ScanAsync ()
		{
			return Task<ScanResult>.Run (() => {
				waitScanResetEvent = new ManualResetEvent (false);

				//Run ScanditActivity
				var intent = new Intent (Forms.Context, typeof(ScanditActivity));
				intent.PutExtra ("licenseKey", licenseKey);

				ScanditActivity.OnCanceled += OnCanceled;
				ScanditActivity.OnScanCompleted += OnScanCompleted;

				Forms.Context.StartActivity (intent);

				waitScanResetEvent.WaitOne ();

				ScanditActivity.OnCanceled -= OnCanceled;
				ScanditActivity.OnScanCompleted -= OnScanCompleted;

				return scanResult;
			});
		}	

		private void OnCanceled(){
			scanResult = new ScanResult () {
				Success = false
			};
			waitScanResetEvent.Set ();
		}

		private void OnScanCompleted(ScanResult result){
			scanResult = result;
			waitScanResetEvent.Set ();
		}
	}

	[Activity(Label = "ScanditActivity", ConfigurationChanges=ConfigChanges.Orientation|ConfigChanges.KeyboardHidden|ConfigChanges.ScreenLayout)]
	public class ScanditActivity: FragmentActivity, IScanditSDKListener {

		private ScanditSDKBarcodePicker picker;

		protected override void OnCreate (Bundle savedInstanceState)
		{
			base.OnCreate (savedInstanceState);

			RequestWindowFeature(WindowFeatures.NoTitle);
			Window.SetFlags(WindowManagerFlags.Fullscreen, WindowManagerFlags.Fullscreen);

			string licenseKey = Intent.GetStringExtra ("licenseKey");

			picker = new ScanditSDKBarcodePicker (this, licenseKey, ScanditSDK.CameraFacingBack);

			picker.OverlayView.AddListener (this);

			picker.StartScanning ();

			SetContentView (picker);
		}

		public void DidCancel ()
		{
			this.Finish ();
			if (OnCanceled != null) {
				OnCanceled ();
			}
		}

		public override void OnBackPressed ()
		{
			base.OnBackPressed ();
			DidCancel ();
		}

		protected override void OnResume ()
		{
			base.OnResume ();
			picker.StartScanning ();
		}

		protected override void OnPause ()
		{
			picker.StopScanning ();
			base.OnPause ();
		}

		public void DidManualSearch (string text)
		{
			//Do nothing
		}

		public void DidScanBarcode (string type, string barcode)
		{
			this.Finish ();
			if (OnScanCompleted != null) {
				OnScanCompleted (new ScanResult () { Success = true, Code = barcode });
			}
		}

		//
		// Static Events
		//
		public static event Action OnCanceled;

		public static event Action<ScanResult> OnScanCompleted;
	}
}

