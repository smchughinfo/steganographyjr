extern alias CoreCompat;

using CoreCompat::System.Drawing.Imaging;
using SteganographyJr.Forms.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Drawing = CoreCompat.System.Drawing;

[assembly: Xamarin.Forms.DependencyAttribute(typeof(SteganographyJr.UWP.Services.DependencyService.Bitmap))]
namespace SteganographyJr.UWP.Services.DependencyService
{
    class Bitmap : Core.DomainObjects.Bitmap
    {
        private Drawing.Bitmap platformBitmap;

        public override void Set(string fileName, Stream stream)
        {
            OriginalFormat = GetImageFormat(fileName);

            platformBitmap = new Drawing.Bitmap(stream);
            platformBitmap.GetPixel(0, 0); // TODO: this is magic line that prevents calls to GetPixel from crashing when doing the encode
        }

        public override int Height {
            get { return platformBitmap.Height; }
        }
        public override int Width {
            get { return platformBitmap.Width; }
        }

        public override Stream ConvertToStream()
        {
            try
            {
                MemoryStream memoryStream = new MemoryStream();

                var platformFormat = GetPlatformFormat();

                ImageCodecInfo jpgEncoder = GetEncoder(ImageFormat.Jpeg);
                var encoderParameters = new EncoderParameters(1) { };
                //encoderParameters.Param.Append(new EncoderParameter(Encoder.Quality, 100));
                encoderParameters.Param[0] = new EncoderParameter(Encoder.Compression, (long)EncoderValue.CompressionNone);

                //platformBitmap.Save(memoryStream, platformFormat);
                platformBitmap.Save(memoryStream, jpgEncoder, encoderParameters);
                memoryStream.Position = 0;

                return memoryStream;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        private ImageCodecInfo GetEncoder(ImageFormat format)
        {
            ImageCodecInfo[] codecs = ImageCodecInfo.GetImageDecoders();
            foreach (ImageCodecInfo codec in codecs)
            {
                if (codec.FormatID == format.Guid)
                {
                    return codec;
                }
            }
            return null;
        }
        private Drawing.Imaging.ImageFormat GetPlatformFormat()
        {
            if(OriginalFormat == Core.DomainObjects.ImageFormat.ImageFormatType.png)
            {
                return Drawing.Imaging.ImageFormat.Png;
            }
            else if(OriginalFormat == Core.DomainObjects.ImageFormat.ImageFormatType.jpg)
            {
                return Drawing.Imaging.ImageFormat.Jpeg;
            }
            else if (OriginalFormat == Core.DomainObjects.ImageFormat.ImageFormatType.gif)
            {
                return Drawing.Imaging.ImageFormat.Gif;
            }

            throw new NotImplementedException($"SteganographyJr.UWP.Services.DependencyService.Bitmap.GetPlatformImageFormat does not recognize the original image format '{OriginalFormat.ToString()}'.");
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