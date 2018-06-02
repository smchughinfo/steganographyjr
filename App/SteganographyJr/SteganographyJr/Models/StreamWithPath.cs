using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SteganographyJr.Models
{
    public class StreamWithPath
    {
        public string Path { get; set; }
        public Stream Stream { get; set; }

        public byte[] GetBytes()
        {
            using (var memoryStream = new MemoryStream())
            {
                Stream.CopyTo(memoryStream);
                return memoryStream.ToArray();
            }
        }
    }
}
