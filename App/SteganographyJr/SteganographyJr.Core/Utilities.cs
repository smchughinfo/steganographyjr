using System;
using System.Collections.Generic;
using System.Text;

namespace SteganographyJr.Core
{
    public static class Utilities
    {
        // https://stackoverflow.com/questions/281640/how-do-i-get-a-human-readable-file-size-in-bytes-abbreviation-using-net
        public static string GetHumanReadableFileSize(int byteSize)
        {
            string[] sizes = { "B", "KB", "MB", "GB", "TB" };
            int order = 0;
            while (byteSize >= 1024 && order < sizes.Length - 1)
            {
                order++;
                byteSize = byteSize / 1024; // this is conservative. if system uses 1000 it will report a smaller file size than is available.
            }

            // Adjust the format string to your preferences. For example "{0:0.#}{1}" would
            // show a single decimal place, and no space.
            string result = String.Format("{0:0.##} {1}", byteSize, sizes[order]);

            return result;
        }
    }
}
