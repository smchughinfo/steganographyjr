﻿using System;
using System.Collections.Generic;
using System.Text;

namespace SteganographyJr
{
    static class StaticVariables
    {
        public enum Modes { Encode, Decode };
        public const string defaultTerminatingString = "0785AFAB-52E8-4356-A8F4-31CACB590B88";
    }
}