using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SteganographyJr.Core.ExtensionMethods
{
    public static class StreamExtensions
    {
        public static byte[] ConvertToByteArray(this Stream stream)
        {
            using (var memoryStream = new MemoryStream())
            {
                stream.CopyTo(memoryStream);
                return memoryStream.ToArray();
            }
        }
    }
}
