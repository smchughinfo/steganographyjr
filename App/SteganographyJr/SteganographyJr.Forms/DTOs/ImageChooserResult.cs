using SteganographyJr.Core;
using SteganographyJr.Core.Classes;
using SteganographyJr.Forms.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SteganographyJr.Forms.DTOs
{
    public class ImageChooserResult
    {
        public string ErrorMessage { get; set; }
        public string Path { get; set; }
        public ImageFormat CarrierImageFormat { get; set; }
        public Stream Stream { get; set; }
        public object NativeRepresentation { get; set; }
    }
}
