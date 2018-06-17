using System;
using System.Collections.Generic;
using System.Text;

namespace SteganographyJr.Core.ExtensionMethods
{
    public static class StringExtensions
    {
        public static byte[] ConvertToByteArray(this string theString)
        {
            return Encoding.UTF8.GetBytes(theString);
        }
    }
}
