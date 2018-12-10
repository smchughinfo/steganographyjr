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
        const int  EOF_CHECK_RATE = 100;

        public static async Task<byte[]> Decode(Bitmap carrierImage, Func<double, bool> checkCancel)
        {
            var eofMarker = _defaultPassword.ConvertToByteArray();
            return await Decode(carrierImage, eofMarker, checkCancel);
        }

        public static async Task<byte[]> Decode(Bitmap carrierImage, byte[] eofMarker, Func<double, bool> checkCancel)
        {
            var shuffleSeed = FisherYates.GetSeed(_defaultPassword);

            bool foundEof = false;
            bool userCancelled = false;

            List<bool> messageBuilder = new List<bool>();
            byte[] decodedMessage = null;

            var userUpdateStopwatch = new Stopwatch();
            var eofStopwatch = new Stopwatch();
            userUpdateStopwatch.Start();
            eofStopwatch.Start();

            await Task.Run(() => // move away from the calling thread while working
            {
                IterateBitmap(carrierImage, shuffleSeed, (x, y) => {
                    var pixelBitsAsBools = DecodePixel(carrierImage, x, y);
                    messageBuilder.AddRange(pixelBitsAsBools);
                    
                    if(eofStopwatch.ElapsedMilliseconds > EOF_CHECK_RATE)
                    {
                        eofStopwatch.Restart();
                        foundEof = MessageBuilderHasEof(messageBuilder, eofMarker);
                    }

                    var percentComplete = (double)messageBuilder.Count / carrierImage.BitCapacity;
                    userCancelled = CheckCancelAndUpdate(userUpdateStopwatch, percentComplete, checkCancel);

                    return userCancelled || foundEof;
                });

                foundEof = MessageBuilderHasEof(messageBuilder, eofMarker); // check for eof again in case the IterateBitmap loop completed since the last time we checked.
                if (foundEof)
                {
                    decodedMessage = GetMessageWithoutEof(messageBuilder, eofMarker);
                }
            });
            
            return userCancelled || !foundEof ? null : decodedMessage;
        }

        public static async Task<byte[]> Take(Bitmap bitmap, string password, Int64 numberOfBitsToTake)
        {
            return await Take(bitmap, password, 0, numberOfBitsToTake);
        }

        public static async Task<byte[]> Take(Bitmap bitmap, string password, int startingByteIndex, Int64 bytesToTake)
        {
            int startingIndexInBits = startingByteIndex * 8;
            int bitsToTake = (int)bytesToTake * 8; // TODO: casting down to int here limits the size of the payload to??? same with the starting index argument? just test these on large images once you add the bit depth switcher please.

            // TODO: this needs all the stopwatch and user cancel stuff
            List<bool> messageBuilder = new List<bool>();

            await Task.Run(() => // move away from the calling thread while working
            {
                var shuffleSeed = FisherYates.GetSeed(password);
                IterateBitmap(bitmap, shuffleSeed, (x, y) =>
                {
                    var pixelBits = DecodePixel(bitmap, x, y);
                    messageBuilder.AddRange(pixelBits);

                    return messageBuilder.Count >= startingIndexInBits + bitsToTake; 
                });
            });

            messageBuilder.RemoveRange(0, startingIndexInBits);
            messageBuilder = messageBuilder.Take(bitsToTake).ToList(); // TODO: does this ever blow up if the image is ..small? or something. ...on encode it can check. here we could get an error?
            return messageBuilder.ConvertToByteArray();
        }

        private static bool MessageBuilderHasEof(List<bool> messageBuilder, byte[] eofMarker)
        {
            return messageBuilder.ConvertToByteArray().Contains(eofMarker);
        }

        private static bool[] DecodePixel(Bitmap carrierImage, int x, int y)
        {
            (int a, int r, int g, int b) = carrierImage.GetPixel(x, y);

            var rBit = r % 2 == 0;
            var gBit = g % 2 == 0;
            var bBit = b % 2 == 0;

            return new bool[] { rBit, gBit, bBit };
        }

        private static byte[] GetMessageWithoutEof(List<bool> messageBuilder, byte[] eofBytes)
        {
            var messageBuilderBytes = messageBuilder.ConvertToByteArray();
            return messageBuilderBytes.SplitOnce(eofBytes)[0];
        }
    }
}
