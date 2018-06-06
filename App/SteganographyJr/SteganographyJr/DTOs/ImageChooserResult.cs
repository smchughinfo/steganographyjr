using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SteganographyJr.DTOs
{
    public class ImageChooserResult
    {
        public string ErrorMessage { get; set; }
        public string Path { get; set; }
        public Stream Stream { get; set; }
        public object NativeRepresentation { get; set; }
    }
}
