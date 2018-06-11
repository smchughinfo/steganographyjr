using SteganographyJr.Services.DependencyService;
using SteganographyJr.DTOs;
using SteganographyJr.Interfaces;
using SteganographyJr.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;
using SteganographyJr.Mvvm;
using SteganographyJr.Services.Steganography;
using SteganographyJr.ExtensionMethods;
using SteganographyJr.Services;
using System.Diagnostics;
using SteganographyJr.Classes;

namespace SteganographyJr.ViewModels
{
    class SteganographyJr : ObservableObject, IViewModel
    {
        byte[] _carrierImageBytes;
        string _carrierImagePath;   // TODO: android + ios + uwp ....does it make sense to keep track of the path? just keep track of the extension? how difficult to read that from the stream?
        CarrierImageFormat _carrierImageFormat;
        ImageSource _carrierImageSource;
        object _carrierImageNative; // a native representation of the carrier image file, if needed, for the platform to resave.

        List<Mode> _modes;
        Mode _selectedMode;

        bool _usePassword;
        string _password;

        List<Message> _messages;      // text or file
        Message _SelectedMessageType; // text or file, whichever is selected
        string _textMessage;          // if text is selected, the text the user entered
        BytesWithPath _fileMessage;   // if file is selected, the file the user selected

        bool _changingCarrierImage;
        public DelegateCommand ChangeCarrierImageCommand { get; private set; }

        bool _changingMessageFile;
        public DelegateCommand ChangeMessageFileCommand { get; private set; }

        bool _executing;
        bool _cancelling;
        public DelegateCommand ExecuteCommand { get; private set; }
        double _executionProgress;

        Steganography _steganography;

        public SteganographyJr()
        {
            InitSteganography();
            InitCarrierImage();
            InitMode();
            InitMessage();
            InitExecute();
            InitPassword(); // has to go after InitExecute so objects exist
            BindDependencies();
        }

        private void InitSteganography()
        {
            _steganography = new Steganography();

            _steganography.ProgressChanged += (object sender, double progress) =>
            {
                ExecutionProgress = progress;
            };
        }

        private void BindDependencies()
        {
            WhenPropertyChanges(() => ChangingCarrierImage)
                .AlsoInvokeAction(ChangeCarrierImageCommand.ChangeCanExecute);

            WhenPropertyChanges(() => ChangingMessageFile)
                .AlsoInvokeAction(ChangeMessageFileCommand.ChangeCanExecute);

            WhenPropertyChanges(() => Executing)
                .AlsoInvokeAction(ExecuteCommand.ChangeCanExecute)
                .AlsoRaisePropertyChangedFor(() => NotExecuting)
                .AlsoRaisePropertyChangedFor(() => ExecutionMessage);

            WhenPropertyChanges(() => NotExecuting)
                .AlsoRaisePropertyChangedFor(() => EnablePassword);

            WhenPropertyChanges(() => CarrierImageBytes)
                .AlsoInvokeAction(UpdateCarrierImageSource)
                .AlsoRaisePropertyChangedFor(() => MessageCapacity);

            WhenPropertyChanges(() => SelectedMode)
                .AlsoInvokeAction(ExecuteCommand.ChangeCanExecute)
                .AlsoRaisePropertyChangedFor(() => SelectedModeIsEncode)
                .AlsoRaisePropertyChangedFor(() => SelectedModeIsDecode)
                .AlsoRaisePropertyChangedFor(() => ShowTextMessage)
                .AlsoRaisePropertyChangedFor(() => ShowFileMessage);

            WhenPropertyChanges(() => UsePassword)
                .AlsoInvokeAction(ExecuteCommand.ChangeCanExecute)
                .AlsoRaisePropertyChangedFor(() => EnablePassword)
                .AlsoRaisePropertyChangedFor(() => DisablePassword);

            WhenPropertyChanges(() => SelectedMessageType)
                .AlsoInvokeAction(ExecuteCommand.ChangeCanExecute)
                .AlsoRaisePropertyChangedFor(() => UsingTextMessage)
                .AlsoRaisePropertyChangedFor(() => ShowTextMessage)
                .AlsoRaisePropertyChangedFor(() => UsingFileMessage)
                .AlsoRaisePropertyChangedFor(() => ShowFileMessage);

            WhenPropertyChanges(() => Password)
                .AlsoInvokeAction(ExecuteCommand.ChangeCanExecute);

            WhenPropertyChanges(() => TextMessage)
                .AlsoInvokeAction(ExecuteCommand.ChangeCanExecute);

            WhenPropertyChanges(() => FileMessage)
                .AlsoInvokeAction(ExecuteCommand.ChangeCanExecute);
        }

        private void InitCarrierImage()
        {
            var assembly = (typeof(SteganographyJr)).GetTypeInfo().Assembly;

            using (var imageStream = assembly.GetManifestResourceStream(StaticVariables.DefaultCarrierImageResource))
            {
                CarrierImageBytes = imageStream.ConvertToByteArray();
                _carrierImageFormat = StaticVariables.DefaultCarrierImageFormat;
            }
            
            UpdateCarrierImageSource();

            _changingCarrierImage = false;
            ChangeCarrierImageCommand = new DelegateCommand(
                execute: PickCarrierImage,
                canExecute: () =>
                {
                    return NotExecuting && !ChangingCarrierImage;
                }
            );
        }

        public bool ChangingCarrierImage {
            get { return _changingCarrierImage; }
            set { SetPropertyValue(ref _changingCarrierImage, value); }
        }

        private void InitMode()
        {
            Modes = new List<Mode>()
            {
                new Mode() { Key=StaticVariables.Mode.Encode, Value="Encode" },
                new Mode() { Key=StaticVariables.Mode.Decode, Value="Decode" }
            };
            SelectedMode = Modes.Single(m => m.Key == StaticVariables.Mode.Encode);
        }

        private void InitPassword()
        {
            UsePassword = false;
            Password = "";
        }

        private void InitMessage()
        {
            Messages = new List<Message>()
            {
                new Message() { Key=StaticVariables.Message.Text, Value="Text" },
                new Message() { Key=StaticVariables.Message.File, Value="File" }
            };

            TextMessage = "Type your message here";

            SelectedMessageType = Messages.Single(m => m.Key == StaticVariables.Message.Text);

            _changingMessageFile = false;
            ChangeMessageFileCommand = new DelegateCommand(
                execute: PickFileMessage,
                canExecute: () =>
                {
                    return NotExecuting && !ChangingMessageFile;
                }
            );
        }

        private async void PickCarrierImage()
        {
            ChangingCarrierImage = true;

            var imageChooserResult = await DependencyService.Get<IFileIO>().GetFileAsync(true);
            if(imageChooserResult != null)
            {
                var success = string.IsNullOrEmpty(imageChooserResult.ErrorMessage);
                if (success)
                {
                    CarrierImageBytes = imageChooserResult.Stream.ConvertToByteArray();
                    CarrierImagePath = imageChooserResult.Path;
                    _carrierImageNative = imageChooserResult.NativeRepresentation;
                    _carrierImageFormat = imageChooserResult.CarrierImageFormat;

                    imageChooserResult.Stream.Dispose();
                }
                else
                {
                    SendOpenFileErrorMessage(imageChooserResult.ErrorMessage);
                }
            }

            ChangingCarrierImage = false;
        }

        private async void PickFileMessage()
        {
            ChangingMessageFile = true;

            var imageChooserResult = await DependencyService.Get<IFileIO>().GetFileAsync();
            if(imageChooserResult != null)
            {
                var success = String.IsNullOrEmpty(imageChooserResult.ErrorMessage);
                if (success)
                {
                    FileMessage = new BytesWithPath()
                    {
                        Bytes = imageChooserResult.Stream.ConvertToByteArray(),
                        Path = imageChooserResult.Path
                    };
                    imageChooserResult.Stream.Dispose();
                }
                else
                {
                    SendOpenFileErrorMessage(imageChooserResult.ErrorMessage);
                }
            }

            ChangingMessageFile = false;
        }

        public bool ChangingMessageFile {
            get { return _changingMessageFile; }
            set { SetPropertyValue(ref _changingMessageFile, value); }
        }

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
            if(_cancelling)
            {
                return;
            }

            _cancelling = true;
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            while(true)
            {
                // this is more a safety check than anything. it should cancel.
                // ...but when it doesn't don't let it crash the computer.
                if(stopwatch.ElapsedMilliseconds > 30000)
                {
                    SendErrorMessage("Unable to cancel. You may need to close this program manually.");
                    return;
                }
                if(_cancelling == false)
                {
                    return;
                }
                await Task.Delay(100);
            }
        }

        private bool CheckCancel()
        {
            bool cancelling = _cancelling;
            if(_cancelling)
            {
                _cancelling = false;
            }
            return cancelling;
        }

        public bool Executing {
            get { return _executing; }
            set { SetPropertyValue(ref _executing, value); }
        }

        public bool NotExecuting {
            get { return !Executing; }
        }

        public string ExecutionMessage {
            get {
                return Executing ? "Cancel" : "Execute";
            }
        }

        public byte[] CarrierImageBytes {
            get { return _carrierImageBytes; }
            set { SetPropertyValue(ref _carrierImageBytes, value); }
        }

        public ImageSource CarrierImageSource {
            get { return _carrierImageSource; }
            set {
                if(value == null) {
                    return;
                }
                SetPropertyValue(ref _carrierImageSource, value);
            }
        }

        private string CarrierImagePath {
            get { return _carrierImagePath; }
            set { SetPropertyValue(ref _carrierImagePath, value); }
        }

        private void UpdateCarrierImageSource()
        {
            CarrierImageSource = ImageSource.FromStream(() => new MemoryStream(CarrierImageBytes));
        }

        public List<Mode> Modes {
            get { return _modes; }
            set { SetPropertyValue(ref _modes, value); }
        }

        public Mode SelectedMode {
            get { return _selectedMode; }
            set { SetPropertyValue(ref _selectedMode, value); }
        }

        public bool SelectedModeIsEncode {
            get { return SelectedMode.Key == StaticVariables.Mode.Encode; }
        }

        public bool SelectedModeIsDecode {
            get { return SelectedMode.Key == StaticVariables.Mode.Decode; }
        }

        public bool UsePassword {
            get { return _usePassword; }
            set { SetPropertyValue(ref _usePassword, value); }
        }

        public bool EnablePassword {
            get { return UsePassword && NotExecuting; }
        }

        public bool DisablePassword
        {
            get { return !EnablePassword; }
        }

        public string Password {
            get { return _password; }
            set { SetPropertyValue(ref _password, value); }
        }

        public List<Message> Messages {
            get { return _messages; }
            set { SetPropertyValue(ref _messages, value); }
        }

        public Message SelectedMessageType {
            get { return _SelectedMessageType; }
            set { SetPropertyValue(ref _SelectedMessageType, value); }
        }

        public string TextMessage {
            get { return _textMessage; }
            set { SetPropertyValue(ref _textMessage, value); }
        }

        public BytesWithPath FileMessage {
            get { return _fileMessage; }
            set { SetPropertyValue(ref _fileMessage, value); }
        }

        public bool UsingTextMessage {
            get { return SelectedMessageType.Key == StaticVariables.Message.Text; }
        }

        public bool ShowTextMessage {
            get { return SelectedModeIsEncode && UsingTextMessage; }
        }

        public bool UsingFileMessage {
            get { return SelectedMessageType.Key == StaticVariables.Message.File; }
        }

        public bool ShowFileMessage {
            get { return SelectedModeIsEncode && UsingFileMessage; }
        }

        public string MessageCapacity
        {
            get
            {
                var messageCapacity = _steganography.GetImageCapacityInBits(CarrierImageBytes) / 8;
                var size = Utilities.GetHumanReadableFileSize(messageCapacity);
                return $"Message Capacity: {size}";
            }
        }

        public double ExecutionProgress {
            get { return _executionProgress; }
            set { SetPropertyValue(ref _executionProgress, value); }
        }
        
        private string GetSteganographyPassword()
        {
            return UsePassword ? Password : StaticVariables.DefaultPassword;
        }

        private byte[] GetSteganographyMessage()
        {
            var bytes = new List<byte>();
            if(UsingTextMessage)
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
            
            bytes.Add(UsingTextMessage ? (byte)0: (byte)1);

            return bytes.ToArray();
        }

        private (StaticVariables.Message messageType, object message) ParseSteganographyMessage(byte[] message)
        {
            var type = (StaticVariables.Message)message.Last();
            message = message.Take(message.Count() - 1).ToArray();

            object returnObject;
            if(type == StaticVariables.Message.Text)
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
                var password = Cryptography.GetHash(GetSteganographyPassword());
                var message = Cryptography.Encrypt(GetSteganographyMessage(), password);

                // make sure we can encode
                var messageFits = _steganography.MessageFits(CarrierImageBytes, message, password);
                if (messageFits == false)
                {
                    SendEncodingErrorMessage("Message is too big. Use a bigger image or write a smaller message.");
                    return;
                }

                // do the encode
                using (var imageStream = await _steganography.Encode(CarrierImageBytes, _carrierImageFormat, message, password, CheckCancel))
                {
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
            catch(Exception ex)
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
            var password = Cryptography.GetHash(GetSteganographyPassword());
            byte[] message =  await _steganography.Decode(CarrierImageBytes, password, CheckCancel);
            if(message != null)
            {
                message = Cryptography.Decrypt(message, password);
                ExecutionProgress = 1;
                await Task.Delay(100);

                await RouteDecodedMessage(message);
                
            }

            ExecutionProgress = 0;
        }

        private async Task RouteDecodedMessage(byte[] message)
        {
            if(message == null)
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

        private void SendErrorMessage(string errorMessage)
        {
            var alertMessage = new AlertMessage()
            {
                Title = "Error",
                CancelButtonText = "Okay",
                Message = errorMessage
            };
            MessagingCenter.Send<IViewModel, AlertMessage>(this, StaticVariables.DisplayAlertMessage, alertMessage);
        }

        private void SendDecodedMessage(string decodedMessage)
        {
            var alertMessage = new AlertMessage()
            {
                Title = "Message Decoded",
                CancelButtonText = "Okay",
                Message = $"Decoded message: {decodedMessage}"
            };
            MessagingCenter.Send<IViewModel, AlertMessage>(this, StaticVariables.DisplayAlertMessage, alertMessage);
        }

        private void SendEncodingSuccessMessage(string imagePath)
        {
            var alertMessage = new AlertMessage()
            {
                Title = "Message Encoded",
                CancelButtonText = "Excelsior!",
                Message = $"Image saved to {imagePath}."
            };
            MessagingCenter.Send<IViewModel, AlertMessage>(this, StaticVariables.DisplayAlertMessage, alertMessage);
        }

        private void SendEncodingErrorMessage(string errorMessage)
        {
            var alertMessage = new AlertMessage()
            {
                Title = "Encoding Error",
                CancelButtonText = "Okay",
                Message = errorMessage
            };
            MessagingCenter.Send<IViewModel, AlertMessage>(this, StaticVariables.DisplayAlertMessage, alertMessage);
        }

        private void SendOpenFileErrorMessage(string errorMessage)
        {
            var alertMessage = new AlertMessage()
            {
                Title = "Open File Error",
                CancelButtonText = "Okay",
                Message = errorMessage
            };
            MessagingCenter.Send<IViewModel, AlertMessage>(this, StaticVariables.DisplayAlertMessage, alertMessage);
        }
    }
}
