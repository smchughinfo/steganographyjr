extern alias CoreCompat;

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Drawing = CoreCompat.System.Drawing;

namespace SteganographyJr.ExtensionMethods
{
    static class StreamExtensions
    {
        public static byte[] ConvertToByteArray(this Stream stream)
        {
            using (var memoryStream = new MemoryStream())
            {
                stream.CopyTo(memoryStream);
                stream.Position = 0;
                return memoryStream.ToArray();
            }
        }
    }
}
