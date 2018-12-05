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
            var eofMarker = _defaultEofMarker.ConvertToByteArray();
            return await Decode(carrierImage, eofMarker, checkCancel);
        }

        public static async Task<byte[]> Decode(Bitmap carrierImage, byte[] eofMarker, Func<double, bool> checkCancel)
        {
            var shuffleSeed = FisherYates.GetSeed(eofMarker);

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
