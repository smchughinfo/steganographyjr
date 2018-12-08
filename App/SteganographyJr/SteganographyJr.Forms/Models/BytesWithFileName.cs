using SteganographyJr.Forms.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace SteganographyJr.Forms.Models
{
    class BytesWithFileName
    {
        public string FileName { get; set; }
        public Byte[] Bytes { get; set; }
    }
}
