using SteganographyJr.Core.DomainObjects;
using System;
using System.Collections.Generic;
using System.Text;

namespace SteganographyJr.Core
{
    public static class StaticVariables
    {
        public enum ExecutionType { Encode, Decode };
        public static readonly ImageFormat[] ImageFormats =
        {
            new ImageFormat(".gif"),
            new ImageFormat(".jpg"),
            new ImageFormat(".jpeg"),
            new ImageFormat(".png")
        };
    }
}
