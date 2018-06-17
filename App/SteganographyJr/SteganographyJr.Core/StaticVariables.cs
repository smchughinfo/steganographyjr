using SteganographyJr.Core.Classes;
using System;
using System.Collections.Generic;
using System.Text;

namespace SteganographyJr.Core
{
    public static class StaticVariables
    {
        public static readonly ImageFormat[] ImageFormats =
        {
            new ImageFormat(".gif"),
            new ImageFormat(".jpg"),
            new ImageFormat(".jpeg"),
            new ImageFormat(".png")
        };
    }
}
