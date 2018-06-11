using SteganographyJr.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace SteganographyJr
{
    public static class StaticVariables
    {
        public const string DefaultCarrierImageResource = "SteganographyJr.Images.default-carrier-image.png";
        public static readonly CarrierImageFormat DefaultCarrierImageFormat = new CarrierImageFormat(DefaultCarrierImageResource);
        public const string DefaultCarrierImageSaveName = "Encoded Image.png";
        public enum Mode { Encode, Decode };
        public const string DefaultPassword = "0785AFAB-52E8-4356-A8F4-31CACB590B88";
        public const string FileSeperator = "30CAD34D-6E4D-4207-BC31-AE94D68FD44D";
        public enum Message { Text, File };
        public const string DisplayAlertMessage = "DisplayAlertMessage";
        public static readonly CarrierImageFormat[] CarrierImageFormats =
        {
            new CarrierImageFormat(".gif"),
            new CarrierImageFormat(".jpg"),
            new CarrierImageFormat(".jpeg"),
            new CarrierImageFormat(".png")
        };
        public static string RequestPermissionMessage = "SteganographyJr needs permission to read and write files. Only the files you choose are accessed by SteganographyJr.";
    }
}
