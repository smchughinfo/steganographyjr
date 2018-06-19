using SteganographyJr.Core;
using SteganographyJr.Core.DomainObjects;
using SteganographyJr.Core.ExtensionMethods;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace SteganographyJr.Steganography
{
    public partial class Codec
    {
        public static bool MessageFits(Bitmap carrierImage, byte[] message, byte[] eofMarker)
        {
            return carrierImage.ByteCapacity >= message.Length + eofMarker.Length;
        }

        public static async Task<Bitmap> Encode(Bitmap carrierImage, byte[] message, Func<double, bool> checkCancel)
        {
            var eofMarker = _defaultEofMarker.ConvertToByteArray();
            return await Encode(carrierImage, message, eofMarker, checkCancel);
        }

        public static async Task<Bitmap> Encode(Bitmap carrierImage, byte[] message, byte[] eofMarker, Func<double, bool> checkCancel)
        {
            var shuffleSeed = FisherYates.GetSeed(eofMarker);
            message = message.Append(eofMarker);

            var userCancelled = false;

            var stopwatch = new Stopwatch();
            stopwatch.Start();

            await Task.Run(() => // move away from the calling thread while working
            {
                var bitsWritten = 0;
                IterateBitmap(carrierImage, shuffleSeed, (x, y) => {
                    EncodePixel(carrierImage, message, ref bitsWritten, x, y);

                    var percentComplete = (double)bitsWritten / carrierImage.BitCapacity;
                    userCancelled = CheckCancelAndUpdate(stopwatch, percentComplete, checkCancel);

                    bool encodeComplete = bitsWritten >= message.Length * 8;
                    return userCancelled || encodeComplete;
                });

                
            });

            return userCancelled ? null : carrierImage;
        }

        private static int GetValueToEncodeInChannel(Bitmap carrierImage, byte[] message, int channelValue, int messageIndex)
        {
            if (messageIndex >= message.Length * 8)
            {
                return channelValue;
            }
            else
            {
                var channelValueEven = channelValue % 2 == 0;
                var messageValueEven = message.ConvertToBitArray()[messageIndex];

                var valuesMatch = messageValueEven == channelValueEven;
                channelValue = valuesMatch ? channelValue : channelValue + 1;
                channelValue = channelValue != 256 ? channelValue : 254;

                return channelValue;
            }
        }
        
        private static void EncodePixel(Bitmap carrierImage, byte[] message, ref int bitsWritten, int x, int y)
        {
            (int a, int r, int g, int b) = carrierImage.GetPixel(x, y);

            r = GetValueToEncodeInChannel(carrierImage, message, r, bitsWritten++);
            g = GetValueToEncodeInChannel(carrierImage, message, g, bitsWritten++);
            b = GetValueToEncodeInChannel(carrierImage, message, b, bitsWritten++);

            carrierImage.SetPixel(x, y, a, r, g, b);
        }
    }
}
