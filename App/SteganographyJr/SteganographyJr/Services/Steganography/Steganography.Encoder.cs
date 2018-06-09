extern alias CoreCompat;

using SteganographyJr.ExtensionMethods;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Drawing = CoreCompat.System.Drawing;

namespace SteganographyJr.Services.Steganography
{
    partial class Steganography
    {
        BitArray MessageBits
        {
            get { return new BitArray(_message); }
        }

        public bool MessageFits(byte[] imageBytes, byte[] message)
        {
            var messageCapacity = GetMessageCapacityInBits(imageBytes) / 8;
            return messageCapacity >= message.Length;
        }

        private int GetMessageCapacityInBits(byte[] imageBytes)
        {
            using (var imageStream = new MemoryStream(imageBytes))
            {
                Drawing.Bitmap bitmap = new Drawing.Bitmap(imageStream);
                return GetMessageCapacityInBits(bitmap);
            }
        }

        private int GetMessageCapacityInBits(Drawing.Bitmap bitmap)
        {
            var numPixels = bitmap.Height * bitmap.Width;
            var numBits = numPixels * 3;
            return numBits;
        }

        public async Task<Stream> Encode(byte[] imageBytes, byte[] message, string password)
        {
            // TODO: encrypt password
            message = message.Append(password);
            InitializeFields(ExecutionType.Encode, imageBytes, password, message);

            await Task.Run(() => // move away from the calling thread while working
            {
                IterateBitmap((x, y) => {
                    EncodePixel(x, y);
                    UpdateProgress();

                    return _messageIndex >= _message.Length * 8;
                });
            });

            var encodedStream = _bitmap.ConvertToStream();

            ClearFields();
            return encodedStream;
        }

        // https://stackoverflow.com/questions/281640/how-do-i-get-a-human-readable-file-size-in-bytes-abbreviation-using-net
        public string GetHumanReadableFileSize(byte[] imageBytes)
        {
            //imageBytes
            var len = GetMessageCapacityInBits(imageBytes) / 8;

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
        
        private void EncodePixel(int x, int y)
        {
            var pixel = _bitmap.GetPixel(x, y); // TODO: figure out how to handle jpegs. convert -> dowork -> convert back -> save with 0 compression. or only save as png. jpeg lossless compression

            var a = pixel.A;
            var r = GetValueToEncodeInChannel(pixel.R, _messageIndex++);
            var g = GetValueToEncodeInChannel(pixel.G, _messageIndex++);
            var b = GetValueToEncodeInChannel(pixel.B, _messageIndex++);

            var newPixel = Drawing.Color.FromArgb(a, r, g, b);
            _bitmap.SetPixel(x, y, newPixel);
        }
    }
}
