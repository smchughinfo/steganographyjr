using SteganographyJr.Core;
using SteganographyJr.Core.Classes;
using SteganographyJr.Forms.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace SteganographyJr.Forms
{
    public static class StaticVariables
    {
        // TODO: move as much into .Core as possible
        public const string DefaultCarrierImageResource = "SteganographyJr.Forms.Images.default-carrier-image.png";
        public static readonly ImageFormat DefaultCarrierImageFormat = new ImageFormat(DefaultCarrierImageResource);
        public const string DefaultCarrierImageSaveName = "Encoded Image.png";
        public enum Mode { Encode, Decode };
        public const string DefaultPassword = "0785AFAB-52E8-4356-A8F4-31CACB590B88";
        public const string FileSeperator = "30CAD34D-6E4D-4207-BC31-AE94D68FD44D";
        public enum Message { Text, File };
        public const string DisplayAlertMessage = "DisplayAlertMessage";
        public const string AlertCompleteMessage = "AlertCompleteMessage";
        public static readonly ImageFormat[] ImageFormats =
        {
            new ImageFormat(".gif"),
            new ImageFormat(".jpg"),
            new ImageFormat(".jpeg"),
            new ImageFormat(".png")
        };
        public static string RequestPermissionMessage = "SteganographyJr needs permission to read and write files. Only the files you choose are accessed by SteganographyJr.";
        public static string SaveFailedBecauseOfPermissionsMessage = "Unable to save.";
    }
}
