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
        // https://stackoverflow.com/questions/26870267/generate-integer-based-on-any-given-string-without-gethashcode
        public static int GetMd5HashAsInt(string input)
        {
            MD5 md5Hasher = MD5.Create();
            var hashed = md5Hasher.ComputeHash(input.ConvertToByteArray());
            var ivalue = BitConverter.ToInt32(hashed, 0);
            return ivalue;
        }

        // https://social.msdn.microsoft.com/Forums/vstudio/en-US/eab7d698-2340-4ba0-a91c-da6fae06963c/aes-encryption-encrypting-byte-array?forum=csharpgeneral
        // https://crypto.stackexchange.com/questions/2280/why-is-the-iv-passed-in-the-clear-when-it-can-be-easily-encrypted
        public static byte[] Encrypt(byte[] bytesToEncrypt, string password)
        {
            byte[] ivSeed = Guid.NewGuid().ToByteArray();
            
            var rfc = new Rfc2898DeriveBytes(password, ivSeed);
            byte[] Key = rfc.GetBytes(16);
            byte[] IV = rfc.GetBytes(16);

            byte[] encrypted;
            using (MemoryStream mstream = new MemoryStream())
            {
                using (AesCryptoServiceProvider aesProvider = new AesCryptoServiceProvider())
                {
                    using (CryptoStream cryptoStream = new CryptoStream(mstream, aesProvider.CreateEncryptor(Key, IV), CryptoStreamMode.Write))
                    {
                        cryptoStream.Write(bytesToEncrypt, 0, bytesToEncrypt.Length);
                    }
                }
                encrypted = mstream.ToArray();
            }

            var messageLengthAs32Bits = Convert.ToInt32(bytesToEncrypt.Length);
            var messageLength = BitConverter.GetBytes(messageLengthAs32Bits);

            encrypted = encrypted.Prepend(ivSeed);
            encrypted = encrypted.Prepend(messageLength);

            return encrypted;
        }

        public static byte[] Decrypt(byte[] bytesToDecrypt, string password)
        {
            (byte[] messageLengthAs32Bits, byte[] bytesWithIv) = bytesToDecrypt.Shift(4); // get the message length
            (byte[] ivSeed, byte[] encrypted) = bytesWithIv.Shift(16);                    // get the initialization vector

            var length = BitConverter.ToInt32(messageLengthAs32Bits, 0);

            var rfc = new Rfc2898DeriveBytes(password, ivSeed);
            byte[] Key = rfc.GetBytes(16);
            byte[] IV = rfc.GetBytes(16);

            byte[] decrypted;
            using (MemoryStream mStream = new MemoryStream(encrypted))
            {
                using (AesCryptoServiceProvider aesProvider = new AesCryptoServiceProvider())
                {
                    aesProvider.Padding = PaddingMode.Zeros;
                    using (CryptoStream cryptoStream = new CryptoStream(mStream,aesProvider.CreateDecryptor(Key, IV), CryptoStreamMode.Read))
                    {
                        cryptoStream.Read(encrypted, 0, length);
                    }
                }
                decrypted = mStream.ToArray().Take(length).ToArray();
            }
            return decrypted;
        }
    }
}
