using SteganographyJr.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace SteganographyJr.Models
{
    class BytesWithPath
    {
        public string Path { get; set; }
        public Byte[] Bytes { get; set; }
    }
}
