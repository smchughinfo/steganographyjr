using System;
using System.Collections.Generic;
using System.Text;

namespace SteganographyJr.Services
{
    static class BitmapEnumerators
    {
        public static int[] FisherYates(int size)
        {
            // https://en.wikipedia.org/wiki/Fisher%E2%80%93Yates_shuffle
            int[] source = GenerateSequentialArray(size);

            int[] a = new int[size];
            var random = new Random(DateTime.Now.Millisecond);
            for (int i = 0; i < size; i++)
            {
                int j = random.Next(0, i + 1);
                if(i != j)
                {
                    a[i] = a[j];
                }
                a[j] = source[i];
            }

            return a;
        }

        static int[] GenerateSequentialArray(int size)
        {
            int[] source = new int[size];
            for (var i = 0; i < size; i++)
            {
                source[i] = i;
            }
            return source;
        }
    }
}
