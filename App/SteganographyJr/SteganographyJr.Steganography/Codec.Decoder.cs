using SteganographyJr.Core.DomainObjects;
using SteganographyJr.Core.ExtensionMethods;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SteganographyJr.Steganography
{
    public static partial class Codec
    {
        public static async Task<byte[]> Take(Bitmap carrierImage, string password, Int64 numberOfBitsToTake, Func<double, bool> checkCancel = null)
        {
            return await Take(carrierImage, password, 0, numberOfBitsToTake, checkCancel);
        }

        public static async Task<byte[]> Take(Bitmap carrierImage, string password, int startingByteIndex, Int64 bytesToTake, Func<double, bool> checkCancel = null)
        {
            var shuffleSeed = FisherYates.GetSeed(password);

            bool userCancelled = false;
            
            List<bool> messageBuilder = new List<bool>();

            int startingIndexInBits = startingByteIndex * 8;
            int bitsToTake = (int)bytesToTake * 8;

            var userUpdateStopwatch = new Stopwatch();
            var eofStopwatch = new Stopwatch();
            userUpdateStopwatch.Start();
            eofStopwatch.Start();

            await Task.Run(() => // move away from the calling thread while working
            {
                IterateBitmap(carrierImage, shuffleSeed, (x, y) =>
                {
                    var pixelBits = DecodePixel(carrierImage, x, y);
                    messageBuilder.AddRange(pixelBits);

                    var recoveredAllBits = messageBuilder.Count >= startingIndexInBits + bitsToTake;
                    var percentComplete = (double)messageBuilder.Count / carrierImage.BitCapacity;
                    userCancelled = checkCancel == null ? false : CheckCancelAndUpdate(userUpdateStopwatch, percentComplete, checkCancel);

                    return recoveredAllBits || userCancelled;
                });
            });

            messageBuilder.RemoveRange(0, startingIndexInBits);
            messageBuilder = messageBuilder.Take(bitsToTake).ToList(); // TODO: does this ever blow up if the image is ..small? or something. ...on encode it can check. here we could get an error?
            return messageBuilder.ConvertToByteArray();
        }

        private static bool[] DecodePixel(Bitmap carrierImage, int x, int y)
        {
            (int a, int r, int g, int b) = carrierImage.GetPixel(x, y);

            var rBit = r % 2 == 0;
            var gBit = g % 2 == 0;
            var bBit = b % 2 == 0;

            return new bool[] { rBit, gBit, bBit };
        }
    }
}
