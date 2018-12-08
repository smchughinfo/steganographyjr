using SteganographyJr.Core;
using SteganographyJr.Core.DomainObjects;
using SteganographyJr.Forms.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SteganographyJr.Forms.DTOs
{
    public class FileChooserResult
    {
        public string ErrorMessage { get; set; }
        public string FileName { get; set; }
        public ImageFormat CarrierImageFormat { get; set; } // TODO: any way to get this out of here?
        public Stream Stream { get; set; }
        public object NativeRepresentation { get; set; } // TODO: this also makes no sense in a general context. 
    }
}
