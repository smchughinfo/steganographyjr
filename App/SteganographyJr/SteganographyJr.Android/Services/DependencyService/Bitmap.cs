using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using SteganographyJr.Models;
using SteganographyJr.Services.DependencyService;

[assembly: Xamarin.Forms.DependencyAttribute(typeof(SteganographyJr.Droid.Services.DependencyService.Bitmap))]
namespace SteganographyJr.Droid.Services.DependencyService
{
    // https://developer.xamarin.com/api/type/Android.Graphics.Bitmap/
    class Bitmap : Classes.Bitmap
    {
        public override void Set(Stream stream)
        {

        }

        public override int Height { get { return 123;  } }
        public override int Width { get { return 456; } }

        public override Stream ConvertToStream()
        {
            return null;
        }

        public override void ChangeFormat(CarrierImageFormat.ImageFormat imageFormat)
        {

        }

        public override (int a, int r, int g, int b) GetPixel(int x, int y)
        {
            return (1, 2, 3, 4);
        }

        public override void SetPixel(int x, int y, int a, int r, int g, int b)
        {

        }
    }
}