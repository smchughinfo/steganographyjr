using SteganographyJr.Core.DomainObjects;
using SteganographyJr.Core.ExtensionMethods;
using SteganographyJr.Cryptography;
using SteganographyJr.Forms.Interfaces;
using SteganographyJr.Forms.Models;
using SteganographyJr.Forms.Mvvm;
using SteganographyJr.Steganography;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace SteganographyJr.Forms.ViewModels
{
    partial class CodecViewModel
    {
        bool _executing;
        bool _cancelling;

        public DelegateCommand ExecuteCommand { get; private set; }
        double _executionProgress;

        private void InitExecute()
        {
            _executing = false;
            _cancelling = false;
            ExecuteCommand = new DelegateCommand(
                execute: async () =>
                {
                    if (!Executing)
                    {
                        await Execute();
                    }
                    else
                    {
                        await Cancel();
                    }
                },
                canExecute: () =>
                {
                    bool passwordOkay = !UsePassword || !string.IsNullOrEmpty(Password);
                    bool textMessageOkay = ShowTextMessage && !string.IsNullOrEmpty(TextMessage);
                    bool fileMessageOkay = ShowFileMessage && FileMessage != null;

                    bool encodingOkay = SelectedModeIsDecode || textMessageOkay || fileMessageOkay;

                    return passwordOkay && encodingOkay;
                }
            );
        }

        private async Task Execute()
        {
            Executing = true;

            if (SelectedModeIsEncode)
            {
                await Encode();
            }
            else
            {
                await Decode();
            }

            Executing = false;
        }

        private async Task Cancel()
        {
            if (_cancelling)
            {
                return;
            }

            _cancelling = true;
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            while (true)
            {
                // this is more a safety check than anything. it should cancel.
                // ...but when it doesn't don't let it crash the computer.
                if (stopwatch.ElapsedMilliseconds > 30000)
                {
                    SendErrorMessage("Unable to cancel. You may need to close this program manually.");
                    return;
                }
                if (_cancelling == false)
                {
                    return;
                }
                await Task.Delay(100);
            }
        }

        private bool CheckCancel(double percentComplete)
        {
            ExecutionProgress = percentComplete;

            bool cancelling = _cancelling;
            if (_cancelling)
            {
                _cancelling = false;
            }
            return cancelling;
        }

        public bool Executing
        {
            get { return _executing; }
            set { SetPropertyValue(ref _executing, value); }
        }

        public bool NotExecuting
        {
            get { return !Executing; }
        }

        public string ExecutionMessage
        {
            get
            {
                return Executing ? "Cancel" : "Execute";
            }
        }

        private string GetSteganographyPassword()
        {
            return UsePassword ? Password : StaticVariables.DefaultPassword;
        }

        private byte[] GetSteganographyMessage()
        {
            var bytes = new List<byte>();
            if (UsingTextMessage)
            {
                var textMessageBytes = TextMessage.ConvertToByteArray();
                bytes.AddRange(textMessageBytes);
            }
            else
            {
                var fileNameString = Path.GetFileName(FileMessage.Path);
                var fileNameBytes = fileNameString.ConvertToByteArray();
                var fileSeperatorBytes = StaticVariables.FileSeperator.ConvertToByteArray();

                bytes.AddRange(fileNameBytes);
                bytes.AddRange(fileSeperatorBytes);
                bytes.AddRange(FileMessage.Bytes);
            }

            bytes.Add(UsingTextMessage ? (byte)0 : (byte)1);

            return bytes.ToArray();
        }

        private Bitmap GetSteganographyBitmap()
        {
            Bitmap bitmap = DependencyService.Get<Bitmap>(DependencyFetchTarget.NewInstance);
            using (var carrierImageStream = CarrierImageBytes.ConvertToStream())
            {
                bitmap.Set(carrierImageStream);
            }
            return bitmap;
        }

        private (StaticVariables.Message messageType, object message) ParseSteganographyMessage(byte[] message)
        {
            var type = (StaticVariables.Message)message.Last();
            message = message.Take(message.Count() - 1).ToArray();

            object returnObject;
            if (type == StaticVariables.Message.Text)
            {
                returnObject = message.ConvertToString();
            }
            else
            {
                returnObject = message;
            }

            return (type, returnObject);
        }

        private async Task Encode()
        {
            try
            {
                // get encoding variables
                var password = SHA2.GetHash(GetSteganographyPassword());
                var message = AES.Encrypt(GetSteganographyMessage(), password);
                var carrierImage = GetSteganographyBitmap();

                // make sure we can encode
                var messageFits = Codec.MessageFits(carrierImage, message, password); // TODO: experiment with not providing an eof
                if (messageFits == false)
                {
                    SendEncodingErrorMessage("Message is too big. Use a bigger image or write a smaller message.");
                    return;
                }

                // do the encode
                using (var imageStream = await Codec.Encode(carrierImage, message, password, CheckCancel))
                {
                    // TODO: the closing operations here can take a really long time making the progress bar appear to just hang at 100%.
                    // TODO: prepending message length and only read when you have that many bits will probably speed up decoding by a lot.
                    if (imageStream == null)
                    {
                        // the user cancelled. cleanup and return.
                        ExecutionProgress = 0;
                        return;
                    }
                    else
                    {
                        var result = imageStream.ConvertToByteArray();
                        CarrierImageBytes = result;
                    }
                }

                ExecutionProgress = 1;
                await Task.Delay(100);

                await RouteEncodedMessage();

                ExecutionProgress = 0;
            }
            catch (Exception ex)
            {
                ;
            }
        }

        private async Task RouteEncodedMessage()
        {
            // save the encode
            var encodedCarrierImage = new BytesWithPath()
            {
                Bytes = CarrierImageBytes,
                Path = CarrierImagePath
            };
            var imageSaveResult = await DependencyService.Get<IFileIO>().SaveImageAsync(CarrierImagePath, CarrierImageBytes, _carrierImageNative);

            // notify the user
            var success = string.IsNullOrEmpty(imageSaveResult.ErrorMessage);
            if (success)
            {
                SendEncodingSuccessMessage(imageSaveResult.SaveLocation);
            }
            else
            {
                SendEncodingErrorMessage(imageSaveResult.ErrorMessage);
            }
        }

        private async Task Decode()
        {
            var password = SHA2.GetHash(GetSteganographyPassword());
            var carrierImage = GetSteganographyBitmap();

            byte[] message = await Codec.Decode(carrierImage, password, CheckCancel);
            if (message != null)
            {
                message = AES.Decrypt(message, password);
                ExecutionProgress = 1;
                await Task.Delay(100);

                await RouteDecodedMessage(message);

            }

            ExecutionProgress = 0;
        }

        private async Task RouteDecodedMessage(byte[] message)
        {
            if (message == null)
            {
                SendErrorMessage("No message found. Are you using the right password?");
                return;
            }

            (StaticVariables.Message messageType, object messageObj) result = ParseSteganographyMessage(message);
            if (result.messageType == StaticVariables.Message.Text)
            {
                var stringMessage = (string)result.messageObj;
                SendDecodedMessage(stringMessage);
            }
            else
            {
                var messageObjAsBytes = (byte[])result.messageObj;
                var seperatorBytes = StaticVariables.FileSeperator.ConvertToByteArray();

                var messageComponents = messageObjAsBytes.Split(seperatorBytes);

                var fileName = messageComponents[0].ConvertToString();
                var fileBytes = messageComponents[1];

                var fileSaveResult = await DependencyService.Get<IFileIO>().SaveFileAsync(fileName, fileBytes);
                var success = string.IsNullOrEmpty(fileSaveResult.ErrorMessage);
                if (success == false)
                {
                    SendErrorMessage(fileSaveResult.ErrorMessage);
                }
            }
        }

        public double ExecutionProgress
        {
            get { return _executionProgress; }
            set { SetPropertyValue(ref _executionProgress, value); }
        }
    }
}
