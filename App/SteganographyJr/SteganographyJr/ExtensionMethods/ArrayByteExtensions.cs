using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

        public static byte[][] Split(this byte[] composite, byte[] seperator)
        {
            int i = 0;
            for(; i < composite.Length - seperator.Length; i++)
            {
                var compositeView = new byte[seperator.Length];
                Array.Copy(composite, i, compositeView, 0, seperator.Length);

                var found = compositeView.SequenceEqual(seperator);
                if (found) break;
            }

            var component1Length = i;
            var component1 = new byte[component1Length];

            var component2Length = composite.Length - seperator.Length - component1Length;
            var component2 = new byte[component2Length];
            var component2Index = i + seperator.Length;

            Array.Copy(composite, 0, component1, 0, component1Length);
            Array.Copy(composite, component2Index, component2, 0, component2Length);

            return new byte[][]
            {
                component1,
                component2
            };
        }

        public static string ConvertToString(this byte[] bytes)
        {
            return Encoding.UTF8.GetString(bytes);
        }
    }
}
