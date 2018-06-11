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
using SteganographyJr.ExtensionMethods;
using SteganographyJr.Models;
using SteganographyJr.Classes;

namespace SteganographyJr.Services.Steganography
{
    partial class Steganography
    {
        // BEGIN CLEAR THESE FIELDS
        ExecutionType? _executionType;
        Bitmap _bitmap;
        byte[] _message;
        Stopwatch _uiStopwatch;
        int _messageIndex;
        List<bool> _messageBuilder;
        CarrierImageFormat _carrierImageFormat;
        // END CLEAR THESE FIELDS

        enum ExecutionType { Encode, Decode };
        public event EventHandler<double> ProgressChanged;
        const int UPDATE_RATE = 100;

        private void InitializeFields(ExecutionType executionType, byte[] imageBytes)
        {
            var carrierImageFormat = new CarrierImageFormat(CarrierImageFormat.ImageFormat.png);
            InitializeFields(executionType, imageBytes, carrierImageFormat, null);
        }

        private void InitializeFields(ExecutionType executionType, byte[] imageBytes, CarrierImageFormat carrierImageFormat, byte[] message)
        {
            _executionType = executionType;
            _message = message;
            _uiStopwatch = new Stopwatch();
            _messageIndex = 0;
            _messageBuilder = new List<bool>();
            _carrierImageFormat = carrierImageFormat;

            using (var imageStream = new MemoryStream(imageBytes))
            {
                _bitmap = Xamarin.Forms.DependencyService.Get<Bitmap>(DependencyFetchTarget.NewInstance);
                _bitmap.Set(imageStream);

                _bitmap.ChangeFormat(CarrierImageFormat.ImageFormat.png);
            }
        }

        private void ClearFields()
        {
            _executionType = null;
            _bitmap = null;
            _message = null;
            _uiStopwatch = null;
            _messageIndex = 0;
            _messageBuilder = null;
            _carrierImageFormat = null;
        }

        private void IterateBitmap(int shuffleSeed, Func<int, int, bool> onPixel)
        {
            var shuffledIndices = FisherYates.Shuffle(shuffleSeed, _bitmap.Height * _bitmap.Width);

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
                double messageCapacity = GetMessageCapacityInBits(_bitmap);
                percentComplete = _messageBuilder.Count / messageCapacity;
            }

            return percentComplete;
        }

        private void UpdateProgress()
        {
            var stopwatchNotRunning = _uiStopwatch.IsRunning == false;

            if (stopwatchNotRunning)
            {
                _uiStopwatch.Start();
            }

            if (_uiStopwatch.ElapsedMilliseconds > UPDATE_RATE)
            {
                var percentComplete = GetPercentComplete();
                ProgressChanged?.Invoke(this, percentComplete);
                Thread.Sleep(0); // keep the ui thread from freezing TODO: ???????????????????
                _uiStopwatch.Restart();
            }
        }
    }
}