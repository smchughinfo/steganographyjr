using SteganographyJr.Core.ExtensionMethods;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace SteganographyJr.Cryptography
{
    public static class AES
    {
        static string eof = "2AA1EC93-063F-40FE-8C2A-D1023A84333E";

        // https://social.msdn.microsoft.com/Forums/vstudio/en-US/eab7d698-2340-4ba0-a91c-da6fae06963c/aes-encryption-encrypting-byte-array?forum=csharpgeneral
        // https://crypto.stackexchange.com/questions/2280/why-is-the-iv-passed-in-the-clear-when-it-can-be-easily-encrypted
        // https://codereview.stackexchange.com/questions/196088/encrypt-a-byte-array
        // https://msdn.microsoft.com/en-us/library/zhe81fz4(v=vs.110).aspx
        public static byte[] Encrypt(byte[] bytesToEncrypt, byte[] password)
        {
            bytesToEncrypt = bytesToEncrypt.Append(eof);

            byte[] ivSeed = GetRandomNumber();
            (byte[] key, byte[] iv) = GetKeyAndIv(password, ivSeed);

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

            encrypted = encrypted.Append(ivSeed);

            return encrypted;
        }

        public static byte[] Decrypt(byte[] bytesToDecrypt, byte[] password)
        {
            (byte[] encrypted, byte[] ivSeed) = bytesToDecrypt.Pop(8);
            (byte[] key, byte[] iv) = GetKeyAndIv(password, ivSeed);

            byte[] decrypted = null;

            try
            {
                using (MemoryStream mStream = new MemoryStream(encrypted))
                {
                    using (AesCryptoServiceProvider aesProvider = new AesCryptoServiceProvider())
                    {
                        aesProvider.Padding = PaddingMode.None;
                        using (CryptoStream cryptoStream = new CryptoStream(mStream, aesProvider.CreateDecryptor(key, iv), CryptoStreamMode.Read))
                        {
                            cryptoStream.Read(encrypted, 0, encrypted.Length);
                        }
                    }

                    // take only the number of bytes that were in the original byte[]
                    decrypted = mStream.ToArray();
                    decrypted = decrypted.SplitOnce(eof.ConvertToByteArray())[0];
                }
            }
            catch (Exception ex)
            {

            }

            return decrypted;
        }

        private static (byte[] key, byte[] iv) GetKeyAndIv(byte[] password, byte[] ivSeed)
        {
            byte[] key = new byte[16];
            byte[] iv = new byte[16];
            using (var rfc = new Rfc2898DeriveBytes(password, ivSeed, 1000)) // 1000 is default. google "how many iterations to use for Rfc2898DeriveBytes"
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
