using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace SteganographyJr.Droid.Plugin.FilePicker.SavePicker
{
    public class SavePickerEventArgs : EventArgs
    {
        Android.Net.Uri uri;
        
        public SavePickerEventArgs(Android.Net.Uri uri)
        {
            Uri = uri;
        }

        public Android.Net.Uri Uri
        {
            get
            {
                return uri;
            }
            set
            {
                uri = value;
            }
        }
    }
}