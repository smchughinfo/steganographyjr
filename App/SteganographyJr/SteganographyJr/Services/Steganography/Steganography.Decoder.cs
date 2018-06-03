using SteganographyJr.ExtensionMethods;
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
        BitArray MessageBits
        {
            get { return new BitArray(_message); }
        }

        public async Task<byte[]> Decode(Stream imageStream, string password)
        {
            InitializeFields(ExecutionType.Decode, imageStream, password);
            var trueEof = Encoding.UTF8.GetBytes(password);

            await Task.Run(() => // move away from the calling thread while working
            {
                IterateBitmap((x, y) => {
                    AddPixelToMessageBuilder(x, y);

                    var foundEof = FoundEof(trueEof);
                    if (foundEof)
                    {
                        ;
                    }

                    UpdateProgress();
                    return foundEof;
                });
            });

            ClearFields();
            return null;
        }

        private void AddPixelToMessageBuilder(int x, int y)
        {
            var pixelBits = DecodePixel(x, y);
            _messageBuilder.AddRange(pixelBits);
        }

        private bool FoundEof(byte[] trueEof)
        {
            var eofCandidate = GetMessageBuilderEofCandidate(trueEof.Length);
            if (eofCandidate == null)
            {
                return false;
            }

            var foundEof = Array.Equals(trueEof, eofCandidate);

            bool isSubset = !eofCandidate.Except(trueEof).Any();
            bool isSubset2 = !trueEof.Except(eofCandidate).Any();
            bool either = isSubset || isSubset2;

            return foundEof; // REMOVE THIS EXTRA CODE
        }

        private byte[] GetMessageBuilderEofCandidate(int trueEofByteLength)
        {
            var messageBytes = _messageBuilder.ConvertToByteArray();

            if (messageBytes.Length < trueEofByteLength)
            {
                return null;
            }

            var messageBits = messageBytes.ConvertToBitArray();
            messageBits = messageBits.Truncate();
            messageBits = messageBits.TakeLastBytes(trueEofByteLength);

            var eofCandidate = messageBits.ConvertToByteArray();
            return eofCandidate;
        }

        private bool[] DecodePixel(int x, int y)
        {
            var pixel = _bitmap.GetPixel(x, y);

            var r = pixel.R % 2 == 0;
            var g = pixel.G % 2 == 0;
            var b = pixel.B % 2 == 0;

            return new bool[] { r, g, b };
        }
    }
}
