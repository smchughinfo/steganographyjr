using System;
using System.Collections.Generic;
using System.Text;

namespace SteganographyJr
{
    static class StaticVariables
    {
        public const string defaultCarrierImageResource = "SteganographyJr.Images.default-carrier-image.png";
        public enum Mode { Encode, Decode };
        public const string defaultPassword = "0785AFAB-52E8-4356-A8F4-31CACB590B88";
        public enum Message { Text, File };
        public const string DisplayAlertMessage = "DisplayAlertMessage";
        // TODO: MAKE SURE STREAMS ALWAYS GET CLOSED. POSSIBLY JUST ADD ONE REFERENCE HERE.
    }
}
