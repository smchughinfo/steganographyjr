using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SteganographyJr.Forms.Models
{
    public class CarrierImageFormat
    {
        public enum ImageFormat { gif, jpg, png };
        public ImageFormat Format {get; }
        public string Extension { get; }

        public CarrierImageFormat(ImageFormat imageFormat)
        {
            Format = imageFormat;
            if(Format == ImageFormat.gif)
            {
                Extension = ".gif";
            }
            else if(Format == ImageFormat.jpg)
            {
                Extension = ".jpg";
            }
            else if(Format == ImageFormat.png)
            {
                Extension = ".png";
            }
        }
        
        /// <summary>
        /// Takes any string, such as a file name or a file path, that ends with .gif, .jpg, .jpeg, or .png
        /// </summary>
        /// <param name="stringWithExtension">.gif, .jpg, .jpeg, or .png</param>
        public CarrierImageFormat(string stringWithDotExtension)
        {
            Extension = "." + stringWithDotExtension.Split('.').Last();
            if(Extension == ".gif")
            {
                Format = ImageFormat.gif;
            }
            else if (Extension == ".jpg" || Extension == ".jpeg")
            {
                Format = ImageFormat.jpg;
            }
            else if(Extension == ".png")
            {
                Format = ImageFormat.png;
            }
        }
    }
}
