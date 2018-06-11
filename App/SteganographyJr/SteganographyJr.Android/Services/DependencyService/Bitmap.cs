using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Graphics;
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
        public Android.Graphics.Bitmap platformBitmap;

        public void Set(byte[] imageBytes)
        {
            var outPadding = new Rect();
            var options = new BitmapFactory.Options() { InMutable = true };
            platformBitmap = BitmapFactory.DecodeByteArray(imageBytes, 0, imageBytes.Length);
        }

        public override void Set(Stream stream)
        {
            var outPadding = new Rect();
            var options = new BitmapFactory.Options() { InMutable = true };
            platformBitmap = BitmapFactory.DecodeStream(stream, outPadding, options);
        }

        public override int Height {
            get { return platformBitmap.Height; }
        }
        public override int Width {
            get { return platformBitmap.Width; }
        }

        public override Stream ConvertToStream()
        {
            MemoryStream memoryStream = new MemoryStream();
            platformBitmap.Compress(Android.Graphics.Bitmap.CompressFormat.Png, 100, memoryStream);
            memoryStream.Position = 0;

            return memoryStream;
        }

        public override void ChangeFormat(CarrierImageFormat.ImageFormat imageFormat)
        {
            Android.Graphics.Bitmap.CompressFormat bitmapImageFormat = null;

            if (imageFormat == CarrierImageFormat.ImageFormat.gif) // TODO: ?gif???
            {
                //bitmapImageFormat = Android.Graphics.Bitmap.CompressFormat.Gi
            }
            else if (imageFormat == CarrierImageFormat.ImageFormat.jpg)
            {
                bitmapImageFormat = Android.Graphics.Bitmap.CompressFormat.Jpeg;
            }
            else if (imageFormat == CarrierImageFormat.ImageFormat.png)
            {
                bitmapImageFormat = Android.Graphics.Bitmap.CompressFormat.Png;
            }

            //platformBitmap = ChangeFormat(bitmapImageFormat);
        }

        // https://developer.android.com/reference/android/graphics/Color
        public override (int a, int r, int g, int b) GetPixel(int x, int y)
        {
            var color = platformBitmap.GetPixel(x, y);
            int a = (color >> 24) & 0xff;
            int r = (color >> 16) & 0xff;
            int g = (color >> 8) & 0xff;
            int b = (color) & 0xff;
            return (a, r, g, b);
        }

        public override void SetPixel(int x, int y, int a, int r, int g, int b)
        {
            Color color = new Color(r, g, b, a);
            platformBitmap.SetPixel(x, y, color);
        }
    }
}