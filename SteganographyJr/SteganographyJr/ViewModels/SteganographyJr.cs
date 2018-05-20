using SteganographyJr.DependencyService;
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

namespace SteganographyJr.ViewModels
{
    class SteganographyJr : INotifyPropertyChanged
    {
        ImageSource carrierImageSource;

        List<Mode> modes;
        Mode selectedMode;

        bool useEncryption;
        string encryptionString;

        bool useCustomTerminatingString;
        string customTerminatingString;

        List<Message> messages;
        Message selectedMessage;
        StreamWithPath fileMessage;

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

                    FileMessage = await Xamarin.Forms.DependencyService.Get<IFilePicker>().GetSteamWithPathAsync();

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
                execute: async () =>
                {
                    executing = true;
                    ((Command)ExecuteCommand).ChangeCanExecute();

                    await Task.Delay(100);
                    ExecutionProgress = 0;
                    await Task.Delay(100);
                    ExecutionProgress = .1;
                    await Task.Delay(100);
                    ExecutionProgress = .2;
                    await Task.Delay(100);
                    ExecutionProgress = .3;
                    await Task.Delay(100);
                    ExecutionProgress = .4;
                    await Task.Delay(100);
                    ExecutionProgress = .5;
                    await Task.Delay(100);
                    ExecutionProgress = .6;
                    await Task.Delay(100);
                    ExecutionProgress = .7;
                    await Task.Delay(100);
                    ExecutionProgress = .8;
                    await Task.Delay(100);
                    ExecutionProgress = .9;
                    await Task.Delay(100);
                    ExecutionProgress = 1;

                    executing = false;
                    ((Command)ExecuteCommand).ChangeCanExecute();
                },
                canExecute: () =>
                {
                    return !executing;
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
