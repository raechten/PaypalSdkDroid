using System;
using Android.Runtime;

namespace PaypalSdkDroid.Payments
{
	public partial class PayPalService
	{
		static IntPtr id_onStartCommand_Landroid_content_Intent_II;
		// Metadata.xml XPath method reference: path="/api/package[@name='com.paypal.android.sdk.payments']/class[@name='PayPalService']/method[@name='onStartCommand' and count(parameter)=3 and parameter[1][@type='android.content.Intent'] and parameter[2][@type='int'] and parameter[3][@type='int']]"
		[Register ("onStartCommand", "(Landroid/content/Intent;II)I", "")]
		public override sealed global::Android.App.StartCommandResult OnStartCommand (global::Android.Content.Intent p0, [global::Android.Runtime.GeneratedEnum] global::Android.App.StartCommandFlags p1, int p2)
		{
			if (id_onStartCommand_Landroid_content_Intent_II == IntPtr.Zero)
				id_onStartCommand_Landroid_content_Intent_II = JNIEnv.GetMethodID (class_ref, "onStartCommand", "(Landroid/content/Intent;II)I");
			global::Android.App.StartCommandResult __ret = (global::Android.App.StartCommandResult)JNIEnv.CallIntMethod  (Handle, id_onStartCommand_Landroid_content_Intent_II, new JValue (p0), new JValue ((int) p1), new JValue (p2));
			return __ret;
		}
	}
}

