﻿using SteganographyJr.Services.DependencyService;
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

namespace SteganographyJr.ViewModels
{
    class SteganographyJr : INotifyPropertyChanged, IViewModel
    {
        Stream carrierImageStream;
        ImageSource carrierImageSource;

        List<Mode> modes;
        Mode selectedMode;

        bool usePassword;
        string password;

        List<Message> messages;     // text or file
        Message selectedMessage;    // text or file, whichever is selected
        StreamWithPath fileMessage; // if file is selected, the file the user selected

        public event PropertyChangedEventHandler PropertyChanged;

        bool changingCarrierImage = false;
        public ICommand ChangeCarrierImageCommand { private set; get; }

        bool changingMessageFile = false;
        public ICommand ChangeMessageFileCommand { private set; get; }

        bool executing = false;
        public ICommand ExecuteCommand { private set; get; }
        double executionProgress;

        public SteganographyJr()
        {
            InitCarrierImage();
            InitMode();
            InitMessage();
            InitExecute();
            InitPassword(); // has to go after InitExecute so objects exist
        }

        private void InitCarrierImage()
        {
            var assembly = (typeof(SteganographyJr)).GetTypeInfo().Assembly;
            CarrierImageStream = assembly.GetManifestResourceStream(StaticVariables.defaultCarrierImageResource);

            ChangeCarrierImageCommand = new Command(
                execute: async () =>
                {
                    ChangingCarrierImage = true;
                    CarrierImageStream = await DependencyService.Get<IPicturePicker>().GetImageStreamAsync();
                    ChangingCarrierImage = false;
                },
                canExecute: () =>
                {
                    return !changingCarrierImage;
                }
            );
        }

        public bool ChangingCarrierImage 
        {
            get
            {
                return changingCarrierImage;
            }
            set
            {
                changingCarrierImage = value;
                ((Command)ChangeCarrierImageCommand).ChangeCanExecute();
            }
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

            ChangeMessageFileCommand = new Command(
                execute: async () =>
                {
                    ChangingMessageFile = true;
                    FileMessage = await DependencyService.Get<IFilePicker>().GetStreamWithPathAsync();
                    ChangingMessageFile = false;
                    
                },
                canExecute: () =>
                {
                    return !changingMessageFile;
                }
            );
        }

        public bool ChangingMessageFile
        {
            get
            {
                return changingMessageFile;
            }
            set
            {
                changingMessageFile = value;
                ((Command)ChangeMessageFileCommand).ChangeCanExecute();
            }
        }

        private void InitExecute()
        {
            ExecuteCommand = new Command(
                execute: async () =>
                {
                    Executing = true;

                    if(selectedMode.Key == StaticVariables.Mode.Encode)
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
                    bool passwordOkay = !usePassword || !string.IsNullOrWhiteSpace(Password);

                    return notExecuting && passwordOkay;
                }
            );
        }

        private bool Executing
        {
            get
            {
                return executing;
            }
            set
            {
                executing = value;
                ((Command)ExecuteCommand).ChangeCanExecute();
            }
        }

        public Stream CarrierImageStream
        {
            get
            {
                return carrierImageStream;
            }
            set
            {
                if(value == null)
                {
                    return;
                }

                carrierImageStream = value;

                var streamCopy = new MemoryStream();
                carrierImageStream.CopyTo(streamCopy);
                streamCopy.Position = 0;

                CarrierImageSource = ImageSource.FromStream(() => streamCopy);
            }
        }

        public ImageSource CarrierImageSource
        {
            get
            {
                return carrierImageSource;
            }
            set
            {
                carrierImageSource = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("CarrierImageSource"));
            }
        }

        public List<Mode> Modes
        {
            get
            {
                return modes;
            }
            set
            {
                modes = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Modes"));
            }
        }

        public Mode SelectedMode
        {
            get
            {
                return selectedMode;
            }
            set
            {
                selectedMode = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("SelectedMode"));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("SelectedModeIsEncode"));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("SelectedModeIsDecode"));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ShowTextMessage"));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ShowFileMessage"));
            }
        }

        public bool SelectedModeIsEncode
        {
            get
            {
                return SelectedMode.Key == StaticVariables.Mode.Encode;
            }
        }

        public bool SelectedModeIsDecode
        {
            get
            {
                return SelectedMode.Key == StaticVariables.Mode.Decode;
            }
        }

        public bool UsePassword
        {
            get
            {
                return usePassword;
            }
            set
            {
                usePassword = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("UsePassword"));
                ((Command)ExecuteCommand).ChangeCanExecute();
            }
        }

        public string Password
        {
            get
            {
                return password;
            }
            set
            {
                password = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Password"));
                ((Command)ExecuteCommand).ChangeCanExecute();
            }
        }

        public List<Message> Messages
        {
            get
            {
                return messages;
            }
            set
            {
                messages = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Messages"));
            }
        }

        public Message SelectedMessage
        {
            get
            {
                return selectedMessage;
            }
            set
            {
                selectedMessage = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("SelectedMessage"));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("UsingTextMessage"));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ShowTextMessage"));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("UsingFileMessage"));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ShowFileMessage"));
            }
        }

        public StreamWithPath FileMessage
        {
            get
            {
                return fileMessage;
            }
            set
            {
                fileMessage = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("FileMessage"));
            }
        }

        public bool UsingTextMessage
        {
            get
            {
                return SelectedMessage.Key == StaticVariables.Message.Text;
            }
        }

        public bool ShowTextMessage
        {
            get
            {
                return SelectedModeIsEncode && UsingTextMessage;
            }
        }

        public bool UsingFileMessage
        {
            get
            {
                return SelectedMessage.Key == StaticVariables.Message.File;
            }
        }

        public bool ShowFileMessage
        {
            get
            {
                return SelectedModeIsEncode && UsingFileMessage;
            }
        }

        public double ExecutionProgress
        {
            get
            {
                return executionProgress;
            }
            set
            {
                executionProgress = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ExecutionProgress"));
            }
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
