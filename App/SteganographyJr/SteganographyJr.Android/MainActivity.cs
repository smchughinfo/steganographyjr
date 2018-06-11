using System;

using Android.App;
using Android.Content.PM;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using Xamarin.Forms;

namespace SteganographyJr.Droid
{
    [Activity(Label = "SteganographyJr", Icon = "@drawable/icon", Theme = "@style/MainTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        // https://forums.xamarin.com/discussion/106938/context-is-obsolete-as-of-version-2-5
        internal static MainActivity Instance { get; private set; }
        internal static string PermissionsChangedMessage = "PermissionsChanged";

        protected override void OnCreate(Bundle bundle)
        {
            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;

            base.OnCreate(bundle);

            global::Xamarin.Forms.Forms.Init(this, bundle);
            LoadApplication(new App());

            Instance = this;
        }

        // https://blog.xamarin.com/requesting-runtime-permissions-in-android-marshmallow/
        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Permission[] grantResults)
        {
            var havePermissions = grantResults[0] == Permission.Granted;
            MessagingCenter.Send(this, PermissionsChangedMessage, havePermissions);
        }
    }
}