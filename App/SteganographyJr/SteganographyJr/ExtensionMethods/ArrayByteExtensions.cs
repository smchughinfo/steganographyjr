using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace SteganographyJr.ExtensionMethods
{
    public static class ArrayByteExtensions
    {
        public static byte[] AppendString(this byte[] message, string stringToAppend)
        {
            return message.AppendString(stringToAppend, Encoding.UTF8);
        }

        public static byte[] AppendString(this byte[] message, string stringToAppend, Encoding encoding)
        {
            var stringBytes = encoding.GetBytes(stringToAppend);
            var arrayWithString = new byte[message.Length + stringBytes.Length];

            message.CopyTo(arrayWithString, 0);
            stringBytes.CopyTo(arrayWithString, message.Length);

            return arrayWithString;
        }
    }
}
