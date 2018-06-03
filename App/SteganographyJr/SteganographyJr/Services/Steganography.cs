extern alias CoreCompat;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Forms;
using Drawing = CoreCompat.System.Drawing;

namespace SteganographyJr.Services
{
    class Steganography
    {
        // BEGIN CLEAR THESE FIELDS
        Drawing.Bitmap _bitmap;
        byte[] _message;
        string _password;
        Stopwatch _stopwatch;
        int _messageIndex;
        // END CLEAR THESE FIELDS

        enum ColorChannel { R, G, B };

        public event EventHandler<double> ProgressChanged;
        const int UPDATE_RATE = 100;

        public bool MessageFits(Stream imageStream, byte[] message)
        {
            var messageCapacity = GetMessageCapacity(imageStream);
            return messageCapacity >= message.Length;
        }

        public async Task<Stream> Encode(Stream imageStream, byte[] message, string password)
        {
            InitializeFields(imageStream, message, password);

            await Task.Run(() => // move away from the calling thread while working
            {
                IterateBitmap((x, y) => {
                    EncodePixel(x, y);
                    UpdateProgress();

                    return _messageIndex >= _message.Length;
                });
            });

            var encodedStream = ConvertBitmapToStream(_bitmap);

            ClearFields();
            return encodedStream;
        }

        public async Task<byte[]> Decode(Stream imageStream, string password)
        {
            ProgressChanged?.Invoke(this, 1);
            await Task.Delay(100);
            ProgressChanged?.Invoke(this, .9);
            await Task.Delay(100);
            ProgressChanged?.Invoke(this, .8);
            await Task.Delay(100);
            ProgressChanged?.Invoke(this, .7);
            await Task.Delay(100);
            ProgressChanged?.Invoke(this, .6);
            await Task.Delay(100);
            ProgressChanged?.Invoke(this, .5);
            await Task.Delay(100);
            ProgressChanged?.Invoke(this, .4);
            await Task.Delay(100);
            ProgressChanged?.Invoke(this, .3);
            await Task.Delay(100);
            ProgressChanged?.Invoke(this, .2);
            await Task.Delay(100);
            ProgressChanged?.Invoke(this, .1);
            await Task.Delay(100);
            ProgressChanged?.Invoke(this, 0);
            await Task.Delay(100);
            
            ClearFields(); // <---------------------- !!!!!!!!!!!!!!!! DONT ERASE THIS
            return null;
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

        private void InitializeFields(Stream imageStream, byte[] message, string password)
        {
            _bitmap = new Drawing.Bitmap(imageStream);
            _message = message;
            _password = password;
            _stopwatch = new Stopwatch();
            _messageIndex = 0;
        }

        private void ClearFields()
        {
            _bitmap = null;
            _message = null;
            _password = null;
            _stopwatch = null;
            _messageIndex = 0;
        }

        private int GetMessageCapacity(Stream imageStream)
        {
            // TODO: pretty sure this is right but it's letting me encode bigger images. double check capacity
            Drawing.Bitmap bitmap = new Drawing.Bitmap(imageStream);
            var numPixels = bitmap.Height * bitmap.Width;
            var numBits = numPixels * 3;
            return numBits;
        }

        private (int x, int y) Get2DCoordinate(int _1DCoordinate, int rows, int columns)
        {
            double row = Math.Floor(_1DCoordinate / (double)columns);
            double col = _1DCoordinate - (row * columns);

            int rowInt = Convert.ToInt32(row);
            int colInt = Convert.ToInt32(col);

            return (x: colInt, y: rowInt);
        }

        private void IterateBitmap(Func<int, int, bool> onPixel)
        {
            var shuffledIndices = FisherYates.Shuffle(_bitmap.Height * _bitmap.Width, _password);

            for (var i = 0; i < shuffledIndices.Length; i++, i++)
            {
                var (x, y) = Get2DCoordinate(shuffledIndices[i], _bitmap.Height, _bitmap.Width);

                var done = onPixel(x, y);
                if (done)
                {
                    break;
                }
            }
        }

        private Stream ConvertBitmapToStream(Drawing.Bitmap bitmap)
        {
            MemoryStream memoryStream = new MemoryStream();
            bitmap.Save(memoryStream, Drawing.Imaging.ImageFormat.Png);
            memoryStream.Position = 0;
            return memoryStream;
        }

        private void EncodePixel(int x, int y)
        {
            var pixel = _bitmap.GetPixel(x, y);

            var a = pixel.A;
            var r = 255;// GetEncodedChannelValue(pixel, ColorChannel.R, _messageIndex++);
            var g = 0;// GetEncodedChannelValue(pixel, ColorChannel.G, _messageIndex++);
            var b = 0;// GetEncodedChannelValue(pixel, ColorChannel.B, _messageIndex++);

            _messageIndex += 3; /////////////////////////////////////////////

            var newPixel = Drawing.Color.FromArgb(a, r, g, b);
            _bitmap.SetPixel(x, y, newPixel);
        }

        private int GetEncodedChannelValue(Drawing.Color color, ColorChannel colorChannel, int messageIndex)
        {
            var channelValue = GetColorChannelValue(color, colorChannel);

            if (messageIndex >= _message.Length)
            {
                return channelValue;
            }
            else
            {
                var messageValue = _message[messageIndex];

                var messageValueEven = messageValue % 2 == 0;
                var channelValueEven = channelValue % 2 == 0;

                var valuesMatch = messageValueEven == channelValueEven;
                channelValue = valuesMatch ? channelValue : channelValue + 1;
                channelValue = channelValue != 256 ? channelValue : 254;

                return channelValue;
            }
            
        }

        private int GetColorChannelValue(Drawing.Color color, ColorChannel colorChannel)
        {
            switch(colorChannel)
            {
                case ColorChannel.R:
                    return color.R;
                case ColorChannel.G:
                    return color.G;
                case ColorChannel.B:
                    return color.B;
                default:
                    return -1;
            }
        }

        private void UpdateProgress()
        {
            var stopwatchNotRunning = _stopwatch.IsRunning == false;
            var percentComplete = (double)_messageIndex / _message.Length;

            if(stopwatchNotRunning)
            {
                _stopwatch.Start();
            }

            if (_stopwatch.ElapsedMilliseconds > UPDATE_RATE)
            {
                ProgressChanged?.Invoke(this, percentComplete);
                Thread.Sleep(0); // keep the ui thread from freezing
                _stopwatch.Restart();
            }
        }
    }

    /*
    private void encode()
    {
        try
        {
            Bitmap image;
            bool doSave = true;
            using (FileStream stream = new FileStream(txtFile.Text, FileMode.Open, FileAccess.ReadWrite))
            {
                image = new Bitmap(stream);
                int h = image.Height;
                int w = image.Width;
                string eof = cbUseEncryption.Checked ? txtEncryption.Text : _eof;
                string text;
                BitArray bitArray;
                bool run = true;
                if (rbMessage.Checked)
                {
                    text = Crypto.EncryptStringAES(txtMessage.Text, eof);
                    bitArray = new BitArray(stringToUnicodeByteArray(text + eof));
                    if (!sizeCheck(text, h, w))
                        run = false;
                }
                else
                {
                    List<byte> _bytes = new List<byte>();

                    byte[] fileBytes = File.ReadAllBytes(txtEmbedFile.Text);
                    _bytes.AddRange(stringToUnicodeByteArray((new FileInfo(txtEmbedFile.Text)).Extension + "." + fileBytes.Length + "." + _eot));
                    _bytes.AddRange(fileBytes);
                    _bytes.AddRange(stringToUnicodeByteArray(eof));
                    bitArray = new BitArray(_bytes.ToArray());
                    int usedBytes = bitArray.Length * 8;
                    if (!sizeCheck(getAvailableBytes(h, w), usedBytes))
                        return;
                }

                if (run)
                {
                    int pos = 0;
                    bool end = false;
                    double numDone = 0;

                    double tot = bitArray.Length;
                    for (int row = 0; row < h; row++)
                    {
                        for (int col = 0; col < w; col++)
                        {
                            Color c = image.GetPixel(col, row);
                            int r = 0;
                            int g = 0;
                            int b = 0;

                            numDone += 3;
                            updatePB(Convert.ToInt32(numDone / tot * 100));
                            //bool doBreak = false;
                            if (pos < bitArray.Length)
                                r = convertPixel(c.R, bitArray.Get(pos++));
                            if (pos < bitArray.Length)
                                g = convertPixel(c.G, bitArray.Get(pos++));
                            if (pos < bitArray.Length)
                                b = convertPixel(c.B, bitArray.Get(pos++));
                            else
                                //doBreak = true;
                                end = true;

                            image.SetPixel(col, row, Color.FromArgb(c.A, r, g, b));
                            //image.SetPixel(col, row, Color.FromArgb(c.A, 255, 0,0));

                            if (end)
                                break;
                        }
                        if (end)
                            break;
                        Thread.Sleep(1);
                    }
                }
                else
                    doSave = false;
            }
            if (doSave)
            {
                image.Save(outDir + "/encoded" + type);
                updatePB(100);
                Thread.Sleep(750);
            }
            crossThreadUpdate(1, "ENCODE COMPLETE!");
            updatePB(0);
        }
        catch (Exception ex)
        {
            updatePB(0);
            crossThreadUpdate(1, "ERROR: " + ex.Message);
        }
    }
    */
}