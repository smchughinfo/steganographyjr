using System;
using System.Collections.Generic;
using System.Text;

namespace SteganographyJr.Core.ExtensionMethods
{
    public static class StringExtensions
    {
        public static byte[] ConvertToByteArray(this string theString)
        {
            return ConvertToByteArray(theString, Encoding.UTF8);
        }

        public static byte[] ConvertToByteArray(this string theString, Encoding encoding)
        {
            return encoding.GetBytes(theString);
        }
    }
}
