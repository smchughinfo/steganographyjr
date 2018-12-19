using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Gif;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System;
using System.Diagnostics;
using System.IO;
using SteganographyJr.Core.ExtensionMethods;

namespace FormatTester
{
    class Program
    {
        static void Main(string[] args)
        {
            var inPath = Path.Combine(Directory.GetCurrentDirectory(), @"Images\jpg.jpg");
            var outPath = Path.Combine(Directory.GetCurrentDirectory(), @"Images\out.jpg.jpg");

            // Image.Load(string path) is a shortcut for our default type. 
            // Other pixel formats use Image.Load<TPixel>(string path))
            using (Image<SixLabors.ImageSharp.PixelFormats.Rgba32> image = Image.Load(inPath))
            {
                /*image.Mutate(x => x
                     .Resize(image.Width / 2, image.Height / 2)
                     .Grayscale());*/

                //image.Mutate(i => i.)

                for(var r = 0; r < image.Height; r++)
                {
                    for(var c = 0; c < image.Width; c++)
                    {
                        var curPixel = image[c, r];

                        if(c % 3 == 0)
                        {
                            var newPixel = new Rgba32(180, 110, 30, 1);
                            image[c, r] = newPixel;
                        }
                    }
                }

                image.Save(outPath);

                //var tmpStream = new MemoryStream();
                //var gifEncoder = new GifEncoder();
                //image.SaveAsGif(tmpStream, gifEncoder);
                //File.WriteAllBytes(outPath, tmpStream.ConvertToByteArray());

                //image.Save(outPath); // Automatic encoder selected based on extension.
            }
        }
    }
}
