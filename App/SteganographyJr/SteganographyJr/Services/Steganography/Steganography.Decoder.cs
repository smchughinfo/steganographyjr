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
        public async Task<byte[]> Decode(byte[] imageBytes, string password)
        {
            InitializeFields(ExecutionType.Decode, imageBytes, password);
            var eof = Cryptography.GetHash(password);

            byte[] decodedMessage = null;
            await Task.Run(() => // move away from the calling thread while working
            {
                IterateBitmap((x, y) => {
                    var pixelBitsAsBools = DecodePixel(x, y);
                    var foundEof = AddBitsAndCheckForEof(pixelBitsAsBools, eof);

                    UpdateProgress();
                    return foundEof;
                });

                decodedMessage = GetMessageWithoutEof(eof);
            });

            ClearFields();
            return decodedMessage;
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
            var pixel = _bitmap.GetPixel(x, y);

            var r = pixel.R % 2 == 0;
            var g = pixel.G % 2 == 0;
            var b = pixel.B % 2 == 0;

            return new bool[] { r, g, b };
        }

        private byte[] GetMessageWithoutEof(byte[] eofBytes)
        {
            var messageBuilderBytes = _messageBuilder.ConvertToByteArray();
            var messageSizeWithoutEof = messageBuilderBytes.Count() - eofBytes.Count();
            return messageBuilderBytes.Take(messageSizeWithoutEof).ToArray();
        }
    }
}
