using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace SteganographyJr.Services
{
    class Cryptography
    {
        // https://stackoverflow.com/questions/26870267/generate-integer-based-on-any-given-string-without-gethashcode
        public static int GetMd5HashAsInt(string input)
        {
            var mystring = "abcd"; // TODO: USE ACTUAL VALUE
            MD5 md5Hasher = MD5.Create();
            var hashed = md5Hasher.ComputeHash(Encoding.UTF8.GetBytes(mystring));
            var ivalue = BitConverter.ToInt32(hashed, 0);
            return ivalue;
        }
    }
}
