using System;
using Android.App;
using Android.Content;
using Android.Widget;
using Android.OS;
using PaypalSdkDroid.Payments;
using Org.Json;
using Java.Math;
using Dbg = System.Diagnostics.Debug;
using Android.Runtime;
using System.Threading.Tasks;

namespace SamplePaypalSdkDroid
{
	[Activity(Label = "SamplePaypalSdkDroid", MainLauncher = true)]
	public class MainActivity : Activity
	{
		private static string ConfigEnvironment = PayPalConfiguration.EnvironmentSandbox;
		// note that these credentials will differ between live & sandbox environments.
		private const string ProductionConfigClientId = @"YOUR PRODUCTION CLIENT ID HERE";
		private const string SandboxConfigClientId = @"YOUR SANDBOX CLIENT ID HERE";
		private const int RequestCodePayment = 1;
		private const int RequestCodeFuturePayment = 2;
		private static PayPalConfiguration config = new PayPalConfiguration();

		protected override void OnCreate(Bundle savedInstanceState)
		{
			base.OnCreate(savedInstanceState);
			SetContentView(Resource.Layout.Main);
				
			config.Environment(ConfigEnvironment);
			var clientId = (ConfigEnvironment == PayPalConfiguration.EnvironmentProduction)
								? ProductionConfigClientId
								: SandboxConfigClientId;

			config.ClientId(clientId);

			//The following are only used in PayPalFuturePaymentActivity.
			config.MerchantName("Hipster Store");
			config.MerchantPrivacyPolicyUri(Android.Net.Uri.Parse("https://www.example.com/privacy"));
			config.MerchantUserAgreementUri(Android.Net.Uri.Parse("https://www.example.com/legal"));

			var intent = new Intent(this, typeof(PayPalService));
			intent.PutExtra(PayPalService.ExtraPaypalConfiguration, config);
			StartService(intent);

			var btnBuyIt = FindViewById<Button>(Resource.Id.buyItBtn);
			var btnFuturePayment = FindViewById<Button>(Resource.Id.futurePaymentBtn);
			var btnFuturePaymentPurchase = FindViewById<Button>(Resource.Id.futurePaymentPurchaseBtn);

			btnBuyIt.Click += OnBuyPressed;
			btnFuturePayment.Click += OnFuturePaymentPressed;
			btnFuturePaymentPurchase.Click += OnFuturePaymentPurchasePressed;
		}

		public void OnBuyPressed(object sender, EventArgs e)
		{
			// PAYMENT_INTENT_SALE will cause the payment to complete immediately.
			// Change PAYMENT_INTENT_SALE to PAYMENT_INTENT_AUTHORIZE to only authorize payment and 
			// capture funds later.
			var thingToBuy = new PayPalPayment(new BigDecimal("99.95"), "USD", "hipster jeans", PayPalPayment.PaymentIntentSale);

			var intent = new Intent(this, typeof(PaymentActivity));
			intent.PutExtra(PaymentActivity.ExtraPayment, thingToBuy);

			StartActivityForResult(intent, RequestCodePayment);
		}

		public void OnFuturePaymentPressed(object sender, EventArgs e)
		{
			var intent = new Intent(this, typeof(PayPalFuturePaymentActivity));
			StartActivityForResult(intent, RequestCodeFuturePayment);
		}

		protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
		{
			if (requestCode == RequestCodePayment)
			{
				if (resultCode == Result.Ok)
				{
					var rawConfirm = data.GetParcelableExtra(PaymentActivity.ExtraResultConfirmation);
					var confirm = rawConfirm.JavaCast<PaymentConfirmation>();
					if (confirm != null)
					{
						try
						{
							SendCompletedPaymentToServer(confirm.Payment);
							Toast.MakeText(ApplicationContext, "PaymentConfirmation info received from PayPal", ToastLength.Long).Show();

						}
						catch (JSONException e)
						{
							Dbg.WriteLine("paymentExample, an extremely unlikely failure occurred: " + e.GetBaseException().Message);
						}
					}
				}
				else if (resultCode == Result.Canceled)
				{
					Dbg.WriteLine("paymentExample, The user canceled.");
				}
				else if ((int)resultCode == PaymentActivity.ResultExtrasInvalid)
				{
					Dbg.WriteLine("paymentExample, An invalid Payment was submitted. Please see the docs.");
				}
			}
			else if (requestCode == RequestCodeFuturePayment)
			{
				if (resultCode == Result.Ok)
				{
					var rawAuth = data.GetParcelableExtra(PayPalFuturePaymentActivity.ExtraResultAuthorization);
					var auth = rawAuth.JavaCast<PayPalAuthorization>();
					if (auth != null)
					{
						try
						{
							Dbg.WriteLine("FuturePaymentExample " + auth.ToJSONObject().ToString(4));

							var authorization_code = auth.AuthorizationCode;
							Dbg.WriteLine("FuturePaymentExample " + authorization_code);

							SendAuthorizationToServer(auth);
							Toast.MakeText(ApplicationContext, "Future Payment code received from PayPal", ToastLength.Long).Show();

						}
						catch (JSONException e)
						{
							Dbg.WriteLine("FuturePaymentExample, an extremely unlikely failure occurred: {0}", e.GetBaseException().Message);
						}
					}
				}
				else if (resultCode == Result.Canceled)
				{
					Dbg.WriteLine("FuturePaymentExample, The user canceled.");
				}
				else if ((int)resultCode == PayPalFuturePaymentActivity.ResultExtrasInvalid)
				{
					Dbg.WriteLine("FuturePaymentExample, Probably the attempt to previously start the PayPalService had an invalid PayPalConfiguration. Please see the docs.");
				}
			}
		}


		private void SendCompletedPaymentToServer (PayPalPayment completedPayment)
		{
			// TODO: Send completedPayment.confirmation to server
			Dbg.WriteLine ("Here is your proof of payment:\n\n{0}\n\nSend this to your server for confirmation and fulfillment.", completedPayment.ToJSONObject().ToString(4));
		}

		private void SendAuthorizationToServer(PayPalAuthorization auth)
		{
			// TODO: Send Authorization to server
			Dbg.WriteLine ("Here is your Authorization:\n\n{0}\n\nSend this to your server for further processing.", auth.ToJSONObject().ToString(4));
		}

		public void OnFuturePaymentPurchasePressed(object sender, EventArgs e)
		{
			// Get the Application Correlation ID from the SDK
			var correlationId = PayPalConfiguration.GetApplicationCorrelationId(this);

			Toast.MakeText(ApplicationContext, string.Format("App Correlation ID received from SDK: {0}", correlationId), ToastLength.Long).Show();
		}

		protected override void OnDestroy()
		{
			// Stop service when done
			StopService(new Intent(this, typeof(PayPalService)));
			base.OnDestroy();
		}
	}
}


