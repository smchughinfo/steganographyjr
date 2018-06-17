using SteganographyJr.Core.ExtensionMethods;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace SteganographyJr.Cryptography
{
    public static class SHA2
    {
        public static byte[] GetHash(string randomString)
        {
            return GetHash(randomString.ConvertToByteArray());
        }

        // https://stackoverflow.com/questions/26870267/generate-integer-based-on-any-given-string-without-gethashcode
        // https://stackoverflow.com/questions/12416249/hashing-a-string-with-sha256
        public static byte[] GetHash(byte[] bytesToHash)
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
