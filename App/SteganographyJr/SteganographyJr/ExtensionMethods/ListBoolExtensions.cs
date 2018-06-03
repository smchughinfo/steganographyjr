using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace SteganographyJr.ExtensionMethods
{
    static class ListBoolExtensions
    {
        public static BitArray ConvertToBitArray(this List<bool> bools)
        {
            var bytes = bools.ConvertToByteArray();
            return new BitArray(bytes);
        }

        public static byte[] ConvertToByteArray(this List<bool> bools)
        {
            var byteSize = Convert.ToInt32(Math.Ceiling(bools.Count / 8f));
            var bytes = new byte[byteSize];
            var bitArray = new BitArray(bools.ToArray());

            bitArray.CopyTo(bytes, 0);

            return bytes;
        }
    }
}
