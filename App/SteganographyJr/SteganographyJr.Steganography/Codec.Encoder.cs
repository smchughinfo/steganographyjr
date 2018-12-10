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
    // TODO: doesnt calculate messageFits correctly on android
    // TODO: cant encode large files. just sits there.
    public partial class Codec
    {
        public static bool MessageFits(Bitmap carrierImage, byte[] message)
        {
            return carrierImage.ByteCapacity >= message.Length;
        }

        public static async Task<Bitmap> Encode(Bitmap carrierImage, byte[] message, Func<double, bool> checkCancel)
        {
            return await Encode(carrierImage, message, _defaultPassword, checkCancel);
        }

        public static async Task<Bitmap> Encode(Bitmap carrierImage, byte[] message, string password, Func<double, bool> checkCancel)
        {
            var shuffleSeed = FisherYates.GetSeed(password);
            message = message.Append(password);

            var messageAsBitArray = message.ConvertToBitArray();

            var userCancelled = false;

            var stopwatch = new Stopwatch();
            stopwatch.Start();

            await Task.Run(() => // move away from the calling thread while working
            {
                var bitsWritten = 0;
                IterateBitmap(carrierImage, shuffleSeed, (x, y) => {
                    EncodePixel(carrierImage, messageAsBitArray, ref bitsWritten, x, y);

                    var percentComplete = ((double)bitsWritten / messageAsBitArray.Length);
                    userCancelled = CheckCancelAndUpdate(stopwatch, percentComplete, checkCancel);

                    bool encodeComplete = bitsWritten >= messageAsBitArray.Length;
                    return userCancelled || encodeComplete;
                });
            });

            return userCancelled ? null : carrierImage;
        }

        private static int GetValueToEncodeInChannel(Bitmap carrierImage, BitArray message, int channelValue, ref int messageIndex)
        {
            if (messageIndex >= message.Length)
            {
                return channelValue;
            }
            else
            {
                var channelValueEven = channelValue % 2 == 0;
                var messageValueEven = message[messageIndex++];

                var valuesMatch = messageValueEven == channelValueEven;
                channelValue = valuesMatch ? channelValue : channelValue + 1;
                channelValue = channelValue != 256 ? channelValue : 254;

                return channelValue;
            }
        }
        
        private static void EncodePixel(Bitmap carrierImage, BitArray message, ref int bitsWritten, int x, int y)
        {
            (int a, int r, int g, int b) = carrierImage.GetPixel(x, y);

            r = GetValueToEncodeInChannel(carrierImage, message, r, ref bitsWritten);
            g = GetValueToEncodeInChannel(carrierImage, message, g, ref bitsWritten);
            b = GetValueToEncodeInChannel(carrierImage, message, b, ref bitsWritten);

            carrierImage.SetPixel(x, y, a, r, g, b);
        }
    }
}
