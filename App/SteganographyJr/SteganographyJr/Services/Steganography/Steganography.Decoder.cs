using SteganographyJr.ExtensionMethods;
using SteganographyJr.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SteganographyJr.Services.Steganography
{
    partial class Steganography
    {
        public async Task<byte[]> Decode(byte[] imageBytes, byte[] password, Func<bool> checkCancel)
        {
            var eof = password; // the password is the eof but callers of this function won't know what eof means.
            var shuffleSeed = FisherYates.GetSeed(eof);

            InitializeFields(ExecutionType.Decode, imageBytes);

            bool userCancelled = false;
            byte[] decodedMessage = null;
            await Task.Run(() => // move away from the calling thread while working
            {
                IterateBitmap(shuffleSeed, (x, y) => {
                    var pixelBitsAsBools = DecodePixel(x, y);
                    var foundEof = AddBitsAndCheckForEof(pixelBitsAsBools, eof);

                    UpdateProgress();

                    userCancelled = checkCancel();
                    return userCancelled || foundEof;
                });
                
                if(userCancelled == false)
                {
                    decodedMessage = GetMessageWithoutEof(eof);
                }
            });

            ClearFields();
            return userCancelled ? null : decodedMessage;
        }

        private bool AddBitsAndCheckForEof(bool[] pixelBitsAsBools, byte[] eof)
        {
            var found = false;
            foreach (var pBool in pixelBitsAsBools)
            {
                _messageBuilder.Add(pBool);        // add just this one bit to the _messageBuilder
                var foundEof = MessageHasEof(eof); // now check if _messageBuilder ends with the eof
                if (foundEof)
                {
                    found = true;
                    break;
                }
            }
            return found;
        }

        private bool MessageHasEof(byte[] eofBytes)
        {
            var messageBuilderBytes = _messageBuilder.ConvertToByteArray();
            var eofCandidateBytes = messageBuilderBytes.Reverse().Take(eofBytes.Length).Reverse().ToArray();

            return eofBytes.SequenceEqual(eofCandidateBytes);
        }

        private bool[] DecodePixel(int x, int y)
        {
            (int a, int r, int g, int b) = _bitmap.GetPixel(x, y);

            var rBit = r % 2 == 0;
            var gBit = g % 2 == 0;
            var bBit = b % 2 == 0;

            return new bool[] { rBit, gBit, bBit };
        }

        private byte[] GetMessageWithoutEof(byte[] eofBytes)
        {
            var messageBuilderBytes = _messageBuilder.ConvertToByteArray();
            var messageSizeWithoutEof = messageBuilderBytes.Count() - eofBytes.Count();
            return messageBuilderBytes.Take(messageSizeWithoutEof).ToArray();
        }
    }
}
