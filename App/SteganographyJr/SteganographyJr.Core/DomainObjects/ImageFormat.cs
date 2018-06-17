using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SteganographyJr.Core.Classes
{
    public class ImageFormat
    {
        public enum ImageFormatType { gif, jpg, png };
        public ImageFormatType Format {get; }
        public string Extension { get; }

        public ImageFormat(ImageFormatType imageFormat)
        {
            Format = imageFormat;
            if(Format == ImageFormatType.gif)
            {
                Extension = ".gif";
            }
            else if(Format == ImageFormatType.jpg)
            {
                Extension = ".jpg";
            }
            else if(Format == ImageFormatType.png)
            {
                Extension = ".png";
            }
        }
        
        /// <summary>
        /// Takes any string, such as a file name or a file path, that ends with .gif, .jpg, .jpeg, or .png
        /// </summary>
        /// <param name="stringWithExtension">.gif, .jpg, .jpeg, or .png</param>
        public ImageFormat(string stringWithDotExtension)
        {
            Extension = "." + stringWithDotExtension.Split('.').Last();
            if(Extension == ".gif")
            {
                Format = ImageFormatType.gif;
            }
            else if (Extension == ".jpg" || Extension == ".jpeg")
            {
                Format = ImageFormatType.jpg;
            }
            else if(Extension == ".png")
            {
                Format = ImageFormatType.png;
            }
        }
    }
}
