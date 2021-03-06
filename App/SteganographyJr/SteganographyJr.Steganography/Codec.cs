﻿using SteganographyJr.Core;
using SteganographyJr.Core.DomainObjects;
using SteganographyJr.Core.ExtensionMethods;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static SteganographyJr.Core.StaticVariables;

namespace SteganographyJr.Steganography
{
    public static partial class Codec
    {
        // TODO: casting down to int limits the size of the payload to??? same with the starting index argument? just test these on large images once you add the bit depth switcher please.
        private const string _defaultPassword = "EC951F51-B03B-4007-AAB7-746C16BE1535";
        const int USER_UPDATE_RATE = 100;

        private static void IterateBitmap(Bitmap bitmap, int shuffleSeed, Func<int, int, bool> onPixel)
        {
            var shuffledIndices = FisherYates.Shuffle(shuffleSeed, bitmap.Height * bitmap.Width);

            for (var i = 0; i < shuffledIndices.Length; i++)
            {
                var (x, y) = bitmap.Get2DCoordinate(shuffledIndices[i]);

                var done = onPixel(x, y);
                if (done)
                {
                    break;
                }
            }
        }
        
        private static bool CheckCancelAndUpdate(Stopwatch stopwatch, double percentComplete, Func<double, bool> checkCancel)
        {
            if (stopwatch.ElapsedMilliseconds > USER_UPDATE_RATE) // the purpose of this is to avoid spamming the caller with updates
            {
                stopwatch.Restart();
                return checkCancel(percentComplete);
            }

            return false;
        }
    }
}