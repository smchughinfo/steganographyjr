extern alias CoreCompat;

using CoreCompat::System.Drawing.Imaging;
using SteganographyJr.Forms.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Drawing = CoreCompat.System.Drawing;

[assembly: Xamarin.Forms.DependencyAttribute(typeof(SteganographyJr.UWP.Services.DependencyService.Bitmap))]
namespace SteganographyJr.UWP.Services.DependencyService
{
    class Bitmap : Core.DomainObjects.Bitmap
    {
        public Drawing.Bitmap platformBitmap;

        public override void Set(Stream stream)
        {
            platformBitmap = new Drawing.Bitmap(stream);
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
            platformBitmap.Save(memoryStream, Drawing.Imaging.ImageFormat.Png);
            memoryStream.Position = 0;

            return memoryStream;
        }

        public override void ChangeFormat(Core.DomainObjects.ImageFormat.ImageFormatType imageFormat)
        {
            ImageFormat bitmapImageFormat = null;

            if (imageFormat == Core.DomainObjects.ImageFormat.ImageFormatType.gif)
            {
                bitmapImageFormat = ImageFormat.Gif;
            }
            else if (imageFormat == Core.DomainObjects.ImageFormat.ImageFormatType.jpg)
            {
                bitmapImageFormat = ImageFormat.Jpeg;
            }
            else if (imageFormat == Core.DomainObjects.ImageFormat.ImageFormatType.png)
            {
                bitmapImageFormat = ImageFormat.Png;
            }

            platformBitmap = ChangeFormat(bitmapImageFormat);
        }

        private Drawing.Bitmap ChangeFormat(ImageFormat imageFormat)
        {
            using (var formatStream = new MemoryStream())
            {
                platformBitmap.Save(formatStream, imageFormat);
                return new Drawing.Bitmap(formatStream);
            }
        }

        public override (int a, int r, int g, int b) GetPixel(int x, int y)
        {
            var pixel = platformBitmap.GetPixel(x, y);
            return (pixel.A, pixel.R, pixel.G, pixel.B);
        }

        public override void SetPixel(int x, int y, int a, int r, int g, int b)
        {
            var color = Drawing.Color.FromArgb(a, r, g, b);
            platformBitmap.SetPixel(x, y, color);
        }
    }
}