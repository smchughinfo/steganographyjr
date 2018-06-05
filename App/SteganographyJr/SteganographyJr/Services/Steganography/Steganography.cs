﻿extern alias CoreCompat;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Forms;
using Drawing = CoreCompat.System.Drawing;
using SteganographyJr.ExtensionMethods;

namespace SteganographyJr.Services.Steganography
{
    partial class Steganography
    {
        // BEGIN CLEAR THESE FIELDS
        ExecutionType? _executionType;
        Drawing.Bitmap _bitmap;
        byte[] _message;
        string _password;
        Stopwatch _uiStopwatch;
        int _messageIndex;
        List<bool> _messageBuilder;
        // END CLEAR THESE FIELDS

        enum ExecutionType { Encode, Decode };
        public event EventHandler<double> ProgressChanged;
        const int UPDATE_RATE = 100;

        private void InitializeFields(ExecutionType executionType, byte[] imageBytes, string password, byte[] message = null)
        {
            _executionType = executionType;
            _message = message;
            _password = password;
            _uiStopwatch = new Stopwatch();
            _messageIndex = 0;
            _messageBuilder = new List<bool>();

            using (var imageStream = new MemoryStream(imageBytes))
            {
                _bitmap = new Drawing.Bitmap(imageStream);
            }
        }

        private void ClearFields()
        {
            _executionType = null;
            _bitmap = null;
            _message = null;
            _password = null;
            _uiStopwatch = null;
            _messageIndex = 0;
            _messageBuilder = null;
        }

        private void IterateBitmap(Func<int, int, bool> onPixel)
        {
            var shuffledIndices = FisherYates.Shuffle(_bitmap.Height * _bitmap.Width, _password);

            for (var i = 0; i < shuffledIndices.Length; i++)
            {
                var (x, y) = _bitmap.Get2DCoordinate(shuffledIndices[i]);

                var done = onPixel(x, y);
                if (done)
                {
                    break;
                }
            }
        }

        private double GetPercentComplete()
        {
            double percentComplete;

            if (_executionType == ExecutionType.Encode)
            {
                percentComplete = (double)_messageIndex / _message.Length;
            }
            else
            {
                double messageCapacity = GetMessageCapacity(_bitmap);
                percentComplete = _messageBuilder.Count / messageCapacity;
            }

            return percentComplete;
        }

        private void UpdateProgress()
        {
            var stopwatchNotRunning = _uiStopwatch.IsRunning == false;

            if(stopwatchNotRunning)
            {
                _uiStopwatch.Start();
            }

            if (_uiStopwatch.ElapsedMilliseconds > UPDATE_RATE)
            {
                var percentComplete = GetPercentComplete();
                ProgressChanged?.Invoke(this, percentComplete);
                Thread.Sleep(0); // keep the ui thread from freezing
                _uiStopwatch.Restart();
            }
        }
    }
}