using System;
using System.Collections.Generic;
using System.Text;

namespace SteganographyJr.Services
{
    static class FisherYates
    {
        // https://en.wikipedia.org/wiki/Fisher%E2%80%93Yates_shuffle
        public static int[] Shuffle(int size, string password)
        {
            int[] source = GenerateSequentialArray(size);

            int[] a = new int[size];
            var seed = GetSeed(password);
            var random = new Random(seed);
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

        // https://stackoverflow.com/questions/26870267/generate-integer-based-on-any-given-string-without-gethashcode
        static int GetSeed(string input)
        {
            // since we are losing bytes this is no good for cryptography. but it's fine for the purpose of generating a seed in this context.
            var hashed = Cryptography.GetHash(input);
            var ivalue = BitConverter.ToInt32(hashed, 0);
            return ivalue;
        }
    }
}
