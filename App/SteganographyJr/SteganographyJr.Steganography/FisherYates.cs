using SteganographyJr.Core.ExtensionMethods;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace SteganographyJr.Steganography
{
    static class FisherYates
    {
        // https://en.wikipedia.org/wiki/Fisher%E2%80%93Yates_shuffle
        public static int[] Shuffle(int seed, int size)
        {
            int[] source = GenerateSequentialArray(size);

            int[] a = new int[size];
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
        // https://stackoverflow.com/questions/2351087/what-is-the-best-32bit-hash-function-for-short-strings-tag-names
        public static int GetSeed(byte[] input)
        {
            // use the first 4 bytes of the hashed input to create a random number that is good enough to serve as a seed.
            var hashed =  GetHash(input);
            var seed = BitConverter.ToInt32(hashed, 0);
            return seed;
        }

        // this is a copy of the hash function in SteganographyJr.Cryptography
        // it's copied here so that StegonagraphyJr.Steganography doesn't have a dependency on SteganographyJr.Cryptography
        // in this context this function serves its purpose as is and is not actually being used for cryptography
        // this function is free to evolve seperatley from any hash functions in SteganographyJr.Cryptography.
        private static byte[] GetHash(byte[] bytesToHash)
        {
            var crypt = new SHA256Managed();
            var hash = new StringBuilder();
            byte[] crypto = crypt.ComputeHash(bytesToHash);
            foreach (byte theByte in crypto)
            {
                hash.Append(theByte.ToString("x2"));
            }
            return hash.ToString().ConvertToByteArray();
        }
    }
}
