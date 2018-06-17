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
    public partial class Codec
    {
        public async Task<byte[]> Decode(Bitmap carrierImage, Func<bool> checkCancel)
        {
            var eofMarker = _defaultEofMarker.ConvertToByteArray();
            return await Decode(carrierImage, eofMarker, checkCancel);
        }

        public async Task<byte[]> Decode(Bitmap carrierImage, byte[] eofMarker, Func<bool> checkCancel)
        {
            var shuffleSeed = FisherYates.GetSeed(eofMarker);

            bool userCancelled = false;

            List<bool> messageBuilder = new List<bool>();
            byte[] decodedMessage = null;

            var stopwatch = new Stopwatch();
            stopwatch.Start();

            await Task.Run(() => // move away from the calling thread while working
            {
                IterateBitmap(carrierImage, shuffleSeed, (x, y) => {
                    var pixelBitsAsBools = DecodePixel(carrierImage, x, y);
                    var foundEof = AddBitsAndCheckForEof(messageBuilder, pixelBitsAsBools, eofMarker);

                    var percentComplete = (double)messageBuilder.Count / carrierImage.BitCapacity;
                    UpdateProgress(stopwatch, percentComplete);

                    userCancelled = checkCancel();
                    return userCancelled || foundEof;
                });
                
                if(userCancelled == false)
                {
                    decodedMessage = GetMessageWithoutEof(messageBuilder, eofMarker);
                }
            });
            
            return userCancelled ? null : decodedMessage;
        }

        private bool AddBitsAndCheckForEof(List<bool> messageBuilder, bool[] pixelBitsAsBools, byte[] eofMarker)
        {
            var found = false;
            foreach (var pBool in pixelBitsAsBools)
            {
                messageBuilder.Add(pBool);                               // add just this one bit to the _messageBuilder
                var foundEof = MessageHasEof(messageBuilder, eofMarker); // now check if _messageBuilder ends with the eof
                if (foundEof)
                {
                    found = true;
                    break;
                }
            }
            return found;
        }

        private bool MessageHasEof(List<bool> messageBuilder, byte[] eofMarker)
        {
            var messageBuilderBytes = messageBuilder.ConvertToByteArray();
            var eofCandidateBytes = messageBuilderBytes.Reverse().Take(eofMarker.Length).Reverse().ToArray();

            return eofMarker.SequenceEqual(eofCandidateBytes);
        }

        private bool[] DecodePixel(Bitmap carrierImage, int x, int y)
        {
            (int a, int r, int g, int b) = carrierImage.GetPixel(x, y);

            var rBit = r % 2 == 0;
            var gBit = g % 2 == 0;
            var bBit = b % 2 == 0;

            return new bool[] { rBit, gBit, bBit };
        }

        private byte[] GetMessageWithoutEof(List<bool> messageBuilder, byte[] eofBytes)
        {
            var messageBuilderBytes = messageBuilder.ConvertToByteArray();
            var messageSizeWithoutEof = messageBuilderBytes.Count() - eofBytes.Count();
            return messageBuilderBytes.Take(messageSizeWithoutEof).ToArray();
        }
    }
}
