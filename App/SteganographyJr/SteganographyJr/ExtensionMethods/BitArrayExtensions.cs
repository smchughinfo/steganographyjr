using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SteganographyJr.ExtensionMethods
{
    static class BitArrayExtensions
    {
        public static BitArray Truncate(this BitArray bitArray)
        {
            var bitsToRemove = bitArray.Length % 8;
            var truncatedLength = bitArray.Length - bitsToRemove;

            var tmpArray = new bool[bitArray.Length];
            bitArray.CopyTo(tmpArray, 0);
            tmpArray = tmpArray.Take(truncatedLength).ToArray();

            var truncatedBitArray = new BitArray(tmpArray);

            return truncatedBitArray;
        }

        public static BitArray TakeLastBytes(this BitArray bitArray, int bytesToTake)
        {
            var tmpArray = new bool[bitArray.Length];
            bitArray.CopyTo(tmpArray, 0);

            tmpArray = tmpArray.Reverse().Take(bytesToTake * 8).Reverse().ToArray();
            bitArray = new BitArray(tmpArray);

            return bitArray;
        }

        public static byte[] ConvertToByteArray(this BitArray bitArray)
        {
            var byteSize = Convert.ToInt32(Math.Ceiling(bitArray.Length / 8f));
            var bytes = new byte[byteSize];
            bitArray.CopyTo(bytes, 0);
            return bytes;
        }
    }
}
