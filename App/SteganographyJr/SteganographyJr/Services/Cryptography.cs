using SteganographyJr.ExtensionMethods;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace SteganographyJr.Services
{
    class Cryptography
    {
        static string eof = "2AA1EC93-063F-40FE-8C2A-D1023A84333E";

        // https://stackoverflow.com/questions/26870267/generate-integer-based-on-any-given-string-without-gethashcode
        // you lose some bytes during the conversion so it's not as secure
        public static int GetHashAsInt(string input)
        {
            var hashed = GetHash(input);
            var ivalue = BitConverter.ToInt32(hashed, 0);
            return ivalue;
        }

        // https://stackoverflow.com/questions/26870267/generate-integer-based-on-any-given-string-without-gethashcode
        // https://stackoverflow.com/questions/12416249/hashing-a-string-with-sha256
        public static byte[] GetHash(string randomString)
        {
            var crypt = new SHA256Managed();
            var hash = new StringBuilder();
            byte[] crypto = crypt.ComputeHash(Encoding.UTF8.GetBytes(randomString));
            foreach (byte theByte in crypto)
            {
                hash.Append(theByte.ToString("x2"));
            }
            return hash.ToString().ConvertToByteArray();
        }

        // https://social.msdn.microsoft.com/Forums/vstudio/en-US/eab7d698-2340-4ba0-a91c-da6fae06963c/aes-encryption-encrypting-byte-array?forum=csharpgeneral
        // https://crypto.stackexchange.com/questions/2280/why-is-the-iv-passed-in-the-clear-when-it-can-be-easily-encrypted
        // https://codereview.stackexchange.com/questions/196088/encrypt-a-byte-array
        // https://msdn.microsoft.com/en-us/library/zhe81fz4(v=vs.110).aspx
        public static byte[] Encrypt(byte[] bytesToEncrypt, string password)
        {
            bytesToEncrypt = bytesToEncrypt.Append(eof);

            byte[] ivSeed = GetRandomNumber();
            (byte[] key, byte[] iv) = GetSymmetricKey(ivSeed, password);

            byte[] encrypted;
            using (MemoryStream mstream = new MemoryStream())
            {
                using (AesCryptoServiceProvider aesProvider = new AesCryptoServiceProvider())
                {
                    using (CryptoStream cryptoStream = new CryptoStream(mstream, aesProvider.CreateEncryptor(key, iv), CryptoStreamMode.Write))
                    {
                        cryptoStream.Write(bytesToEncrypt, 0, bytesToEncrypt.Length);
                    }
                }
                encrypted = mstream.ToArray();
            }

            encrypted = encrypted.Prepend(ivSeed);

            return encrypted;
        }

        public static byte[] Decrypt(byte[] bytesToDecrypt, string password)
        {
            (byte[] ivSeed, byte[] encrypted) = bytesToDecrypt.Shift(8);
            (byte[] key, byte[] iv) = GetSymmetricKey(ivSeed, password);

            byte[] decrypted;
            using (MemoryStream mStream = new MemoryStream(encrypted))
            {
                using (AesCryptoServiceProvider aesProvider = new AesCryptoServiceProvider())
                {
                    aesProvider.Padding = PaddingMode.None;
                    using (CryptoStream cryptoStream = new CryptoStream(mStream,aesProvider.CreateDecryptor(key, iv), CryptoStreamMode.Read))
                    {
                        cryptoStream.Read(encrypted, 0, encrypted.Length);
                    }
                }

                // take only the number of bytes that were in the original byte[]
                decrypted = mStream.ToArray();
                decrypted = decrypted.Split(eof.ConvertToByteArray())[0];
            }
            return decrypted;
        }

        private static (byte[] key, byte[] iv) GetSymmetricKey(byte[] ivSeed, string password)
        {
            byte[] key = new byte[16];
            byte[] iv = new byte[16];
            using (var rfc = new Rfc2898DeriveBytes(password, ivSeed))
            {
                key = rfc.GetBytes(16);
                iv = rfc.GetBytes(16);
            }

            return (key, iv);
        }

        private static byte[] GetRandomNumber()
        {
            byte[] salt1 = new byte[8];
            using (RNGCryptoServiceProvider rngCsp = new RNGCryptoServiceProvider())
            {
                // Fill the array with a random value.
                rngCsp.GetBytes(salt1);
            }
            return salt1;
        }
    }
}
