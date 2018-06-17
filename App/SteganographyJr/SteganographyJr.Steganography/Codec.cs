using SteganographyJr.Core;
using SteganographyJr.Core.DomainObjects;
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
        private const string _defaultEofMarker = "EC951F51-B03B-4007-AAB7-746C16BE1535";
        const int UPDATE_RATE = 100;

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
            if (stopwatch.ElapsedMilliseconds > UPDATE_RATE)
            {
                stopwatch.Restart();
                return checkCancel(percentComplete);
            }

            return false;
        }
    }
}