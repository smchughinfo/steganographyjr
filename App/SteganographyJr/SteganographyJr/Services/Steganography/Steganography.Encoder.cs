using SteganographyJr.Classes;
using SteganographyJr.ExtensionMethods;
using SteganographyJr.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace SteganographyJr.Services.Steganography
{
    partial class Steganography
    {
        BitArray MessageBits
        {
            get { return new BitArray(_message); }
        }

        public bool MessageFits(byte[] imageBytes, byte[] message, string password)
        {
            var eof = Cryptography.GetHash(password);
            var messageCapacity = GetMessageCapacityInBits(imageBytes) / 8;
            return messageCapacity >= message.Length + eof.Length;
        }

        private int GetMessageCapacityInBits(byte[] imageBytes)
        {
            using (var imageStream = new MemoryStream(imageBytes))
            {
                var bitmap = Xamarin.Forms.DependencyService.Get<Bitmap>(DependencyFetchTarget.NewInstance);
                bitmap.Set(imageStream);

                return GetMessageCapacityInBits(bitmap);
            }
        }

        private int GetMessageCapacityInBits(Bitmap bitmap)
        {
            var numPixels = bitmap.Height * bitmap.Width;
            var numBits = numPixels * 3;
            return numBits;
        }

        public async Task<Stream> Encode(byte[] imageBytes, CarrierImageFormat carrierImageFormat, byte[] message, string password, Func<bool> checkCancel)
        {
            var eof = Cryptography.GetHash(password);
            var shuffleSeed = FisherYates.GetSeed(eof);

            message = message.Append(eof);
            InitializeFields(ExecutionType.Encode, imageBytes, carrierImageFormat, message);

            bool userCancelled = false;
            await Task.Run(() => // move away from the calling thread while working
            {
                IterateBitmap(shuffleSeed, (x, y) => {
                    EncodePixel(x, y);
                    UpdateProgress();

                    userCancelled = checkCancel();
                    bool encodeComplete = _messageIndex >= _message.Length * 8;
                    return userCancelled || encodeComplete;
                });
            });

            Stream encodedStream = userCancelled ? null : _bitmap.ConvertToStream();

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
            (int a, int r, int g, int b) = _bitmap.GetPixel(x, y);

            r = GetValueToEncodeInChannel(r, _messageIndex++);
            g = GetValueToEncodeInChannel(g, _messageIndex++);
            b = GetValueToEncodeInChannel(b, _messageIndex++);

            _bitmap.SetPixel(x, y, a, r, g, b);
        }
    }
}
