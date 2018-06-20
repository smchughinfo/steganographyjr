using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace SteganographyJr.Core.ExtensionMethods
{
    public static class ArrayByteExtensions
    {
        public static (byte[] left, byte[] right) Pop(this byte[] bytes, int size)
        {
            var left = new byte[bytes.Length - size];
            var right = new byte[size];

            Array.Copy(bytes, 0, left, 0, left.Length);
            Array.Copy(bytes, left.Length, right, 0, right.Length);

            return (left, right);
        }

        public static byte[] Prepend(this byte[] bytes, byte[] bytesToPrepend)
        {
            var newArray = new byte[bytes.Length + bytesToPrepend.Length];
            bytesToPrepend.CopyTo(newArray, 0);
            bytes.CopyTo(newArray, bytesToPrepend.Length);
            return newArray;
        }

        public static byte[] Append(this byte[] bytes, int intToAppend)
        {
            var intAsBytes = BitConverter.GetBytes(intToAppend);
            return bytes.Append(intAsBytes);
        }

        public static byte[] Append(this byte[] bytes, string stringToAppend)
        {
            return bytes.Append(stringToAppend, Encoding.UTF8);
        }

        public static byte[] Append(this byte[] bytes, string stringToAppend, Encoding encoding)
        {
            var stringBytes = encoding.GetBytes(stringToAppend);
            return bytes.Append(stringBytes);
        }

        public static byte[] Append(this byte[] bytes, byte[] bytesToAppend)
        {
            var newArray = new byte[bytes.Length + bytesToAppend.Length];
            bytes.CopyTo(newArray, 0);
            bytesToAppend.CopyTo(newArray, bytes.Length);

            return newArray;
        }

        // https://stackoverflow.com/questions/9755090/split-a-byte-array-at-a-delimiter/50732160#50732160
        public static byte[][] SplitOnce(this byte[] composite, byte[] seperator)
        {
            bool found = false;

            int i = 0;
            for (; i < composite.Length - seperator.Length; i++)
            {
                var compositeView = new byte[seperator.Length];
                Array.Copy(composite, i, compositeView, 0, seperator.Length);

                found = compositeView.SequenceEqual(seperator);
                if (found) break;
            }

            if (found == false)
            {
                return null;
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

        public static bool Contains(this byte[] bytes, byte[] subBytes)
        {
            return bytes.SplitOnce(subBytes) != null;
        }

        public static string ConvertToString(this byte[] bytes)
        {
            return ConvertToString(bytes, Encoding.UTF8);
        }

        public static string ConvertToString(this byte[] bytes, Encoding encoding)
        {
            return encoding.GetString(bytes);
        }

        public static Stream ConvertToStream(this byte[] bytes)
        {
            return new MemoryStream(bytes);
        }

        public static BitArray ConvertToBitArray(this byte[] bytes)
        {
            return new BitArray(bytes);
        }
    }
}
