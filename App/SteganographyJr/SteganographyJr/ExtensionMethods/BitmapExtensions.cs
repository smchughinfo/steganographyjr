extern alias CoreCompat;

using CoreCompat::System.Drawing.Imaging;
using SteganographyJr.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Drawing = CoreCompat.System.Drawing;

namespace SteganographyJr.ExtensionMethods
{
    public static class BitmapExtensions
    {
        public static Stream ConvertToStream(this Drawing.Bitmap bitmap)
        {
            MemoryStream memoryStream = new MemoryStream();
            bitmap.Save(memoryStream, Drawing.Imaging.ImageFormat.Png);
            memoryStream.Position = 0;

            return memoryStream;
        }

        public static (int x, int y) Get2DCoordinate(this Drawing.Bitmap bitmap, int _1DCoordinate)
        {
            double row = Math.Floor(_1DCoordinate / (double)bitmap.Width);
            double col = _1DCoordinate - (row * bitmap.Width);

            int rowInt = Convert.ToInt32(row);
            int colInt = Convert.ToInt32(col);

            return (x: colInt, y: rowInt);
        }

        public static Drawing.Bitmap ChangeFormat(this Drawing.Bitmap bitmap, CarrierImageFormat.ImageFormat imageFormat)
        {
            ImageFormat bitmapImageFormat = null;

            if(imageFormat == CarrierImageFormat.ImageFormat.gif) {
                bitmapImageFormat = ImageFormat.Gif;
            }
            else if (imageFormat == CarrierImageFormat.ImageFormat.jpg) {
                bitmapImageFormat = ImageFormat.Jpeg;
            }
            else if (imageFormat == CarrierImageFormat.ImageFormat.png) {
                bitmapImageFormat = ImageFormat.Png;
            }

            return ChangeFormat(bitmap, bitmapImageFormat);
        }

        public static Drawing.Bitmap ChangeFormat(this Drawing.Bitmap bitmap, ImageFormat imageFormat)
        {
            using (var formatStream = new MemoryStream())
            {
                bitmap.Save(formatStream, imageFormat);
                return new Drawing.Bitmap(formatStream);
            }
        }
    }
}
