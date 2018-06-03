extern alias CoreCompat;

using SteganographyJr.ExtensionMethods;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Drawing = CoreCompat.System.Drawing;

namespace SteganographyJr.Services.Steganography
{
    partial class Steganography
    {
        public bool MessageFits(Stream imageStream, byte[] message, string password)
        {
            var eofLength = Encoding.UTF8.GetBytes(password).Length;
            var messageCapacity = GetMessageCapacity(imageStream);
            return messageCapacity - eofLength >= message.Length;
        }

        private int GetMessageCapacity(Stream imageStream)
        {
            // TODO: pretty sure this is right but it's letting me encode bigger images. double check capacity
            Drawing.Bitmap bitmap = new Drawing.Bitmap(imageStream);
            return GetMessageCapacity(bitmap);
        }

        private int GetMessageCapacity(Drawing.Bitmap bitmap)
        {
            var numPixels = bitmap.Height * bitmap.Width;
            var numBits = numPixels * 3;
            return numBits;
        }

        public async Task<Stream> Encode(Stream imageStream, byte[] message, string password)
        {
            message = message.AppendString(password);
            InitializeFields(ExecutionType.Encode, imageStream, password, message);

            await Task.Run(() => // move away from the calling thread while working
            {
                IterateBitmap((x, y) => {
                    EncodePixel(x, y);
                    UpdateProgress();

                    return _messageIndex >= _message.Length * 8;
                });
            });

            var encodedStream = _bitmap.ConvertToStream();
            var result = encodedValues.ConvertToByteArray();

            ClearFields();
            return encodedStream;
        }

        // https://stackoverflow.com/questions/281640/how-do-i-get-a-human-readable-file-size-in-bytes-abbreviation-using-net
        public string GetHumanReadableFileSize(Stream imageSource)
        {
            // TODO: test this more
            var len = GetMessageCapacity(imageSource) / 8;

            string[] sizes = { "B", "KB", "MB", "GB", "TB" };
            int order = 0;
            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len = len / 1024;
            }

            // Adjust the format string to your preferences. For example "{0:0.#}{1}" would
            // show a single decimal place, and no space.
            string result = String.Format("{0:0.##} {1}", len, sizes[order]);

            return result;
        }

        private int GetValueToEncodeInChannel(int channelValue, int messageIndex)
        {
            if (messageIndex >= _message.Length * 8)
            {
                return channelValue;
            }
            else
            {
                var channelValueEven = channelValue % 2 == 0;
                var messageValueEven = MessageBits[messageIndex];

                var valuesMatch = messageValueEven == channelValueEven;
                channelValue = valuesMatch ? channelValue : channelValue + 1;
                channelValue = channelValue != 256 ? channelValue : 254;

                return channelValue;
            }
        }

        List<bool> encodedValues = new List<bool>();

        private void EncodePixel(int x, int y)
        {
            var pixel = _bitmap.GetPixel(x, y);

            var a = pixel.A;
            var r = GetValueToEncodeInChannel(pixel.R, _messageIndex++);
            var g = GetValueToEncodeInChannel(pixel.G, _messageIndex++);
            var b = GetValueToEncodeInChannel(pixel.B, _messageIndex++);

            encodedValues.Add(r % 2 == 0);
            encodedValues.Add(g % 2 == 0);
            encodedValues.Add(b % 2 == 0);

            var newPixel = Drawing.Color.FromArgb(a, r, g, b);
            _bitmap.SetPixel(x, y, newPixel);
        }
    }
}
