using SteganographyJr.Services.DependencyService;
using SteganographyJr.DTOs;
using SteganographyJr.Interfaces;
using SteganographyJr.Models;
using SteganographyJr.Services;
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

namespace SteganographyJr.ViewModels
{
    class SteganographyJr : ObservableObject, IViewModel
    {
        Stream _carrierImageStream;
        ImageSource _carrierImageSource;

        List<Mode> _modes;
        Mode _selectedMode;

        bool _usePassword;
        string _password;

        List<Message> _messages;      // text or file
        Message _SelectedMessageType; // text or file, whichever is selected
        string _textMessage;          // if text is selected, the text the user entered
        StreamWithPath _fileMessage;  // if file is selected, the file the user selected

        bool _changingCarrierImage;
        public DelegateCommand ChangeCarrierImageCommand { get; private set; }

        bool _changingMessageFile;
        public DelegateCommand ChangeMessageFileCommand { get; private set; }

        bool _executing;
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
                .AlsoRaisePropertyChangedFor(() => NotExecuting);

            WhenPropertyChanges(() => NotExecuting)
                .AlsoRaisePropertyChangedFor(() => EnablePassword);

            WhenPropertyChanges(() => CarrierImageStream)
                .AlsoInvokeAction(UpdateCarrierImageSource);

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
            CarrierImageStream = assembly.GetManifestResourceStream(StaticVariables.defaultCarrierImageResource);
            UpdateCarrierImageSource();

            _changingCarrierImage = false;
            ChangeCarrierImageCommand = new DelegateCommand(
                execute: async () =>
                {
                    ChangingCarrierImage = true;
                    CarrierImageStream = await DependencyService.Get<IPicturePicker>().GetImageStreamAsync();
                    ChangingCarrierImage = false;
                },
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
                new Mode() { Key=StaticVariables.Mode.Encode, Value="Encode"},
                new Mode() { Key=StaticVariables.Mode.Decode, Value="Decode"}
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
                new Message() { Key=StaticVariables.Message.Text, Value="Text"},
                new Message() { Key=StaticVariables.Message.File, Value="File"}
            };

            TextMessage = "Type your message here";

            SelectedMessageType = Messages.Single(m => m.Key == StaticVariables.Message.Text);

            _changingMessageFile = false;
            ChangeMessageFileCommand = new DelegateCommand(
                execute: async () =>
                {
                    ChangingMessageFile = true;
                    FileMessage = await DependencyService.Get<IFilePicker>().GetStreamWithPathAsync();
                    ChangingMessageFile = false;
                    
                },
                canExecute: () =>
                {
                    return NotExecuting && !ChangingMessageFile;
                }
            );
        }

        public bool ChangingMessageFile {
            get { return _changingMessageFile; }
            set { SetPropertyValue(ref _changingMessageFile, value); }
        }

        private void InitExecute()
        {
            _executing = false;
            ExecuteCommand = new DelegateCommand(
                execute: async () =>
                {
                    Executing = true;

                    if(SelectedModeIsEncode)
                    {
                        await Encode();
                    }
                    else
                    {
                        await Decode();
                    }

                    Executing = false;
                },
                canExecute: () =>
                {
                    bool passwordOkay = !UsePassword || !string.IsNullOrEmpty(Password);
                    bool textMessageOkay = ShowTextMessage && !string.IsNullOrEmpty(TextMessage);
                    bool fileMessageOkay = ShowFileMessage && FileMessage != null;

                    bool encodingOkay = SelectedModeIsDecode || textMessageOkay || fileMessageOkay;

                    return NotExecuting && passwordOkay && encodingOkay;
                }
            );
        }

        public bool Executing {
            get { return _executing; }
            set { SetPropertyValue(ref _executing, value); }
        }

        public bool NotExecuting {
            get { return !Executing; }
        }

        public Stream CarrierImageStream {
            get { return _carrierImageStream; }
            set { SetPropertyValue(ref _carrierImageStream, value); }
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

        private void UpdateCarrierImageSource()
        {
            var streamCopy = new MemoryStream();
            CarrierImageStream.CopyTo(streamCopy);
            streamCopy.Position = 0;

            CarrierImageSource = ImageSource.FromStream(() => streamCopy);
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

        public StreamWithPath FileMessage {
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

        public double ExecutionProgress {
            get { return _executionProgress; }
            set { SetPropertyValue(ref _executionProgress, value); }
        }
        
        private string GetSteganographyPassword()
        {
            return UsePassword ? Password : StaticVariables.defaultPassword;
        }

        private byte[] GetSteganographyMessage()
        {
            var bytes = new List<byte>(
                UsingTextMessage ? Encoding.UTF8.GetBytes(TextMessage) : FileMessage.GetBytes()
            );

            var terminatingStringBytes = Encoding.UTF8.GetBytes(GetSteganographyPassword());
            bytes.AddRange(terminatingStringBytes);

            return bytes.ToArray();
        }

        private async Task Encode()
        {
            var password = GetSteganographyPassword();
            var message = GetSteganographyMessage();

            CarrierImageStream = await _steganography.Encode(CarrierImageStream, message, password);

            ExecutionProgress = 100;
            await Task.Delay(1000);
            ExecutionProgress = 0;
        }

        private async Task Decode()
        {
            await Task.Delay(250);
            ExecutionProgress = 1;

            for (var i = 10; i >= 0; i--)
            {
                await Task.Delay(250);
                ExecutionProgress = (double)i / 10;
            }
        }
    }
}
