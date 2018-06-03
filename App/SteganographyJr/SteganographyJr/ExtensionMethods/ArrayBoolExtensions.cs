using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace SteganographyJr.ExtensionMethods
{
    static class ArrayBoolExtensions
    {
        public static byte[] ConvertToByteArray(this bool[] bools)
        {
            byte[] bytes = new byte[Convert.ToInt32(Math.Ceiling(bools.Length / 8f))];
            (new BitArray(bools)).CopyTo(bytes, 0);
            return bytes;
        }
    }
}
