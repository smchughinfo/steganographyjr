﻿using SteganographyJr.Core.ExtensionMethods;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SteganographyJr.Core.DomainObjects
{
    // TODO: think about this. any special methods will prevent system.drawing.common from being plug and play
    public abstract class Bitmap
    {
        public abstract void Set(Stream stream);
        public abstract int Height { get; }
        public abstract int Width { get; }
        public abstract Stream ConvertToStream();
        public abstract void ChangeFormat(ImageFormat.ImageFormatType imageFormat);
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

        public int BitCapacity
        {
            get
            {
                var numPixels = Height * Width;
                var numBits = numPixels * 3;
                return numBits;
            }
        }

        public int ByteCapacity
        {
            get
            {
                return BitCapacity / 8;
            }
        }

        public byte[] ConvertToByteArray()
        {
            using (var stream = ConvertToStream())
            {
                return stream.ConvertToByteArray();
            }
        }
    }
}
