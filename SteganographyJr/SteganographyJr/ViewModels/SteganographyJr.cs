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

namespace SteganographyJr.ViewModels
{
    class SteganographyJr : INotifyPropertyChanged, IViewModel
    {
        ImageSource carrierImageSource;

        List<Mode> modes;
        Mode selectedMode;

        bool useEncryption;
        string encryptionString;

        bool useCustomTerminatingString;
        string customTerminatingString;

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
            InitEncryption();
            InitTerminatingString();
            InitMessage();
            InitExecute();
        }

        private void InitCarrierImage()
        {
            var assembly = (typeof(SteganographyJr)).GetTypeInfo().Assembly;
            carrierImageSource = ImageSource.FromResource(StaticVariables.defaultCarrierImageResource, assembly);

            ChangeCarrierImageCommand = new Command(
                execute: async () =>
                {
                    changingCarrierImage = true;
                    ((Command)ChangeCarrierImageCommand).ChangeCanExecute();

                    var source = await Xamarin.Forms.DependencyService.Get<IPicturePicker>().GetImageStreamAsync();
                    CarrierImageSource = ImageSource.FromStream(() => source);

                    changingCarrierImage = false;
                    ((Command)ChangeCarrierImageCommand).ChangeCanExecute();
                },
                canExecute: () =>
                {
                    return !changingCarrierImage;
                }
            );
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

        private void InitEncryption()
        {
            UseEncryption = false;
            EncryptionString = "";
        }

        private void InitTerminatingString()
        {
            UseCustomTerminatingString = false;
            CustomTerminatingString = "";
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
                    changingMessageFile = true;
                    ((Command)ChangeMessageFileCommand).ChangeCanExecute();

                    FileMessage = await Xamarin.Forms.DependencyService.Get<IFilePicker>().GetStreamWithPathAsync();

                    changingCarrierImage = false;
                    ((Command)ChangeMessageFileCommand).ChangeCanExecute();
                },
                canExecute: () =>
                {
                    return !changingMessageFile;
                }
            );
        }

        private void InitExecute()
        {
            ExecuteCommand = new Command(
                execute: () =>
                {
                    executing = true; // probably only needed if async
                    ((Command)ExecuteCommand).ChangeCanExecute(); // probably only needed if async

                    var encodingError = Steganography.GetFirstEncodingError();
                    if(encodingError != null)
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

                    executing = false; // probably only needed if async
                    ((Command)ExecuteCommand).ChangeCanExecute(); // probably only needed if async
                },
                canExecute: () =>
                {
                    bool notExecuting = !executing;
                    bool encryptionOkay = !UseEncryption;
                    bool terminatingStringOkay = !UseCustomTerminatingString;

                    return notExecuting && encryptionOkay && terminatingStringOkay;
                }
            );
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
            }
        }

        public bool UseEncryption
        {
            get
            {
                return useEncryption;
            }
            set
            {
                useEncryption = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("UseEncryption"));
            }
        }

        public string EncryptionString
        {
            get
            {
                return encryptionString;
            }
            set
            {
                encryptionString = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("EncryptionString"));
            }
        }

        public bool UseCustomTerminatingString
        {
            get
            {
                return useCustomTerminatingString;
            }
            set
            {
                useCustomTerminatingString = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("UseCustomTerminatingString"));
            }
        }

        public string CustomTerminatingString
        {
            get
            {
                return customTerminatingString;
            }
            set
            {
                customTerminatingString = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("CustomTerminatingString"));
            }
        }

        public string TerminatingString
        {
            get
            {
                return useCustomTerminatingString ? customTerminatingString : StaticVariables.defaultTerminatingString;
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
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("UsingFileMessage"));
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

        public bool UsingFileMessage
        {
            get
            {
                return SelectedMessage.Key == StaticVariables.Message.File;
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
    }
}
