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

        List<Message> _messages;     // text or file
        Message _selectedMessage;    // text or file, whichever is selected
        StreamWithPath _fileMessage; // if file is selected, the file the user selected

        bool _changingCarrierImage = false;
        public DelegateCommand ChangeCarrierImageCommand { get; private set; }

        bool _changingMessageFile = false;
        public DelegateCommand ChangeMessageFileCommand { get; private set; }

        bool _executing = false;
        public DelegateCommand ExecuteCommand { get; private set; }
        double _executionProgress;

        public SteganographyJr()
        {
            InitCarrierImage();
            InitMode();
            InitMessage();
            InitExecute();
            InitPassword(); // has to go after InitExecute so objects exist
            BindDependencies();
        }

        private void BindDependencies()
        {
            WhenPropertyChanges(() => ChangingCarrierImage)
                .AlsoInvokeAction(ChangeCarrierImageCommand.ChangeCanExecute);

            WhenPropertyChanges(() => ChangingMessageFile)
                .AlsoInvokeAction(ChangeMessageFileCommand.ChangeCanExecute);

            WhenPropertyChanges(() => Executing)
                .AlsoInvokeAction(ExecuteCommand.ChangeCanExecute);

            WhenPropertyChanges(() => CarrierImageStream)
                .AlsoInvokeAction(UpdateCarrierImageSource);

            WhenPropertyChanges(() => SelectedMode)
                .AlsoRaisePropertyChangedFor(() => SelectedModeIsEncode)
                .AlsoRaisePropertyChangedFor(() => SelectedModeIsDecode)
                .AlsoRaisePropertyChangedFor(() => ShowTextMessage)
                .AlsoRaisePropertyChangedFor(() => ShowFileMessage);

            WhenPropertyChanges(() => UsePassword)
                .AlsoInvokeAction(ExecuteCommand.ChangeCanExecute);

            WhenPropertyChanges(() => Password)
                .AlsoInvokeAction(ExecuteCommand.ChangeCanExecute);

            WhenPropertyChanges(() => SelectedMessage)
                .AlsoRaisePropertyChangedFor(() => UsingTextMessage)
                .AlsoRaisePropertyChangedFor(() => ShowTextMessage)
                .AlsoRaisePropertyChangedFor(() => UsingFileMessage)
                .AlsoRaisePropertyChangedFor(() => ShowFileMessage);
        }

        private void InitCarrierImage()
        {
            var assembly = (typeof(SteganographyJr)).GetTypeInfo().Assembly;
            CarrierImageStream = assembly.GetManifestResourceStream(StaticVariables.defaultCarrierImageResource);
            UpdateCarrierImageSource();

            ChangeCarrierImageCommand = new DelegateCommand(
                execute: async () =>
                {
                    ChangingCarrierImage = true;
                    CarrierImageStream = await DependencyService.Get<IPicturePicker>().GetImageStreamAsync();
                    ChangingCarrierImage = false;
                },
                canExecute: () =>
                {
                    return !ChangingCarrierImage;
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
            SelectedMessage = Messages.Single(m => m.Key == StaticVariables.Message.Text);

            ChangeMessageFileCommand = new DelegateCommand(
                execute: async () =>
                {
                    ChangingMessageFile = true;
                    FileMessage = await DependencyService.Get<IFilePicker>().GetStreamWithPathAsync();
                    ChangingMessageFile = false;
                    
                },
                canExecute: () =>
                {
                    return !ChangingMessageFile;
                }
            );
        }

        public bool ChangingMessageFile {
            get { return _changingMessageFile; }
            set { SetPropertyValue(ref _changingMessageFile, value); }
        }

        private void InitExecute()
        {
            ExecuteCommand = new DelegateCommand(
                execute: async () =>
                {
                    Executing = true;

                    if(SelectedMode.Key == StaticVariables.Mode.Encode)
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
                    bool notExecuting = !Executing; // TODO: these need finished.
                    bool passwordOkay = !UsePassword || !string.IsNullOrWhiteSpace(Password);

                    return notExecuting && passwordOkay;
                }
            );
        }

        private bool Executing {
            get { return _executing; }
            set { SetPropertyValue(ref _executing, value); }
        }

        public Stream CarrierImageStream {
            get { return _carrierImageStream; }
            set { SetPropertyValue(ref _carrierImageStream, value); }
        }

        public ImageSource CarrierImageSource {
            get { return _carrierImageSource; }
            set { SetPropertyValue(ref _carrierImageSource, value); }
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

        public string Password {
            get { return _password; }
            set { SetPropertyValue(ref _password, value); }
        }

        public List<Message> Messages {
            get { return _messages; }
            set { SetPropertyValue(ref _messages, value); }
        }

        public Message SelectedMessage {
            get { return _selectedMessage; }
            set { SetPropertyValue(ref _selectedMessage, value); }
        }

        public StreamWithPath FileMessage {
            get { return _fileMessage; }
            set { SetPropertyValue(ref _fileMessage, value); }
        }

        public bool UsingTextMessage {
            get { return SelectedMessage.Key == StaticVariables.Message.Text; }
        }

        public bool ShowTextMessage {
            get { return SelectedModeIsEncode && UsingTextMessage; }
        }

        public bool UsingFileMessage {
            get { return SelectedMessage.Key == StaticVariables.Message.File; }
        }

        public bool ShowFileMessage {
            get { return SelectedModeIsEncode && UsingFileMessage; }
        }

        public double ExecutionProgress {
            get { return _executionProgress; }
            set { SetPropertyValue(ref _executionProgress, value); }
        }

        private async Task Encode()
        {
            for(var i = 0; i <= 10; i++)
            {
                await Task.Delay(250);
                ExecutionProgress = (double)i / 10;
            }





            /*var encodingError = Steganography.GetFirstEncodingError(CarrierImageStream);
            if (encodingError != null)
            {
                var msg = new AlertMessage()
                {
                    Title = "Encoding Error",
                    Message = encodingError,
                    CancelButtonText = "Okay"
                };
                MessagingCenter.Send<IViewModel, AlertMessage>(this, StaticVariables.DisplayAlertMessage, msg);
            }
            else
            {

            }

            carrierImageStream.Position = 0;
            var st = Steganography.Test(CarrierImageStream);
            st.Position = 0;
            CarrierImageStream = st;
            await Task.Delay(1000);*/
        }

        private async Task Decode()
        {
            await Task.Delay(1000);
        }
    }
}
