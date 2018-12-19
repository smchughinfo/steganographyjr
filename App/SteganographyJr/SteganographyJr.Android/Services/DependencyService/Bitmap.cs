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
using SteganographyJr.Forms;

[assembly: Xamarin.Forms.DependencyAttribute(typeof(SteganographyJr.Droid.Services.DependencyService.Bitmap))]
namespace SteganographyJr.Droid.Services.DependencyService
{
    // https://developer.xamarin.com/api/type/Android.Graphics.Bitmap/
    class Bitmap : Core.DomainObjects.Bitmap
    {
        public Android.Graphics.Bitmap platformBitmap;
        
        public override void Set(string fileName, Stream stream)
        {
            OriginalFormat = GetImageFormat(fileName);

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

            var platformFormat = GetPlatformFormat();
            platformBitmap.Compress(platformFormat, 100, memoryStream);
            memoryStream.Position = 0;

            return memoryStream;
        }

        private Android.Graphics.Bitmap.CompressFormat GetPlatformFormat()
        {
            if (OriginalFormat == Core.DomainObjects.ImageFormat.ImageFormatType.png)
            {
                return Android.Graphics.Bitmap.CompressFormat.Png;
            }
            else if (OriginalFormat == Core.DomainObjects.ImageFormat.ImageFormatType.jpg)
            {
                return Android.Graphics.Bitmap.CompressFormat.Jpeg;
            }
            else if (OriginalFormat == Core.DomainObjects.ImageFormat.ImageFormatType.gif)
            {
                // https://stackoverflow.com/questions/12100314/assigning-gif-data-to-bitmap-object
                // https://stackoverflow.com/questions/24428263/how-to-convert-png-image-to-gif-in-android-programatically
            }

            throw new NotImplementedException($"SteganographyJr.UWP.Services.DependencyService.Bitmap.GetPlatformImageFormat does not recognize the original image format '{OriginalFormat.ToString()}'.");
        }

        public override void ChangeFormat(Core.DomainObjects.ImageFormat.ImageFormatType imageFormat)
        {
            Android.Graphics.Bitmap.CompressFormat bitmapImageFormat = null;

            if (imageFormat == Core.DomainObjects.ImageFormat.ImageFormatType.gif) // TODO: ?gif??? - possibly just dont change the format on android.
            {
                //bitmapImageFormat = Android.Graphics.Bitmap.CompressFormat.Gi
            }
            else if (imageFormat == Core.DomainObjects.ImageFormat.ImageFormatType.jpg)
            {
                bitmapImageFormat = Android.Graphics.Bitmap.CompressFormat.Jpeg;
            }
            else if (imageFormat == Core.DomainObjects.ImageFormat.ImageFormatType.png)
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