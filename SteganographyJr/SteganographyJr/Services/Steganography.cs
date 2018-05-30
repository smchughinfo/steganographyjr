extern alias CoreCompat;

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using Drawing = CoreCompat.System.Drawing;

namespace SteganographyJr.Services
{


    static class Steganography
    {
        public static string GetFirstEncodingError(Stream imageStream)
        {
            var payloadSize = GetMaxPayloadSizeInBits(imageStream);
            // common sense checks like image actually has > 0 pixels

            return "the payload size is " + payloadSize;
        }

        public static int GetMaxPayloadSizeInBits(Stream imageStream)
        {
            Drawing.Bitmap bitmap = new Drawing.Bitmap(imageStream);
            var numPixels = bitmap.Height * bitmap.Width;
            var numBits = numPixels * 3;
            return numBits;
        }

        private static (int x, int y) Get2DCoordinate (int _1DCoordinate, int rows, int columns)
        {
            double row = Math.Floor(_1DCoordinate / (double)columns);
            double col = _1DCoordinate - (row * columns);

            int rowInt = Convert.ToInt32(row);
            int colInt = Convert.ToInt32(col);

            return (x:colInt, y:rowInt);
        }

        private static void IterateBitmap(Drawing.Bitmap bitmap, string password, Action<int, int, int, double> onPixel)
        {
            var shuffledIndices = FisherYates.Shuffle(bitmap.Height * bitmap.Width, password);
            for (var i = 0; i < shuffledIndices.Length; i++)
            {
                var coord = Get2DCoordinate(shuffledIndices[i], bitmap.Height, bitmap.Width);
                var percentComplete = (double)i/shuffledIndices.Length;

                onPixel(i, coord.y, coord.x, percentComplete);
            }
        }

        private static TaskCompletionSource<bool> IterateBitmapAsync(Drawing.Bitmap bitmap, string password, Action<int, int, int, double> onPixel)
        {
            var promise = new TaskCompletionSource<bool>();
            Task.Run(() =>
            {
                IterateBitmap(bitmap, password, async (i, r, c, p) =>
                {
                    onPixel(i, r, c, p);
                    await Task.Delay(0);
                });
                promise.SetResult(true);
            });
            return promise;
        }

        public static async Task Test(Stream imageStream, string password, Action<Stream, double> OnUpdate)
        {
            Drawing.Bitmap bitmap = new Drawing.Bitmap(imageStream);

            await IterateBitmapAsync(bitmap, password, (i, r, c, p) =>
            {
                var pixel = bitmap.GetPixel(c,r);
                var newPixel = Drawing.Color.FromArgb(
                    pixel.A,
                    Math.Abs(255 - pixel.R),
                    Math.Abs(255 - pixel.G),
                    Math.Abs(255 - pixel.B)
                );
                bitmap.SetPixel(c, r, newPixel);

                if ((double)i % 1000 == 0)
                {
                    MemoryStream memoryStream = new MemoryStream();
                    bitmap.Save(memoryStream, Drawing.Imaging.ImageFormat.Png);
                    memoryStream.Position = 0;

                    OnUpdate(memoryStream, p);

                    memoryStream.Close();
                }
            }).Task;

            MemoryStream memoryStream2 = new MemoryStream();
            bitmap.Save(memoryStream2, Drawing.Imaging.ImageFormat.Png);
            memoryStream2.Position = 0;

            OnUpdate(memoryStream2, 1);

            memoryStream2.Close();
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