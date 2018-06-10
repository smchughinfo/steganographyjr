using SteganographyJr.Models;
using SteganographyJr.Services.DependencyService;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Xamarin.Forms;

namespace SteganographyJr.Classes
{
    public abstract class Bitmap
    {
        public abstract void Set(Stream stream);
        public abstract int Height { get; }
        public abstract int Width { get; }
        public abstract Stream ConvertToStream();
        public abstract void ChangeFormat(CarrierImageFormat.ImageFormat imageFormat);
        public abstract (int a, int r, int g, int b) GetPixel(int x, int y);
        public abstract void SetPixel(int x, int y, int a, int r, int g, int b);

        public (int x, int y) Get2DCoordinate(int _1DCoordinate)
        {
            double row = Math.Floor(_1DCoordinate / (double)Width);
            double col = _1DCoordinate - (row * Width);

            int rowInt = Convert.ToInt32(row);
            int colInt = Convert.ToInt32(col);

            return (x: colInt, y: rowInt);
        }
    }
}
