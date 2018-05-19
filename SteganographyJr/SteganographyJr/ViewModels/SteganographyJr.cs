using SteganographyJr.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace SteganographyJr.ViewModels
{
    class SteganographyJr : INotifyPropertyChanged
    {
        List<Mode> modes;
        Mode selectedMode;

        bool useEncryption;
        string encryptionString;

        bool useCustomTerminatingString;
        string customTerminatingString;

        List<Message> messages;
        Message selectedMessage;

        public event PropertyChangedEventHandler PropertyChanged;

        public SteganographyJr()
        {
            Modes = new List<Mode>()
            {
                new Mode() { Key=StaticVariables.Mode.Encode, Value="Encode"},
                new Mode() { Key=StaticVariables.Mode.Decode, Value="Decode"}
            };
            SelectedMode = Modes.Single(m => m.Key == StaticVariables.Mode.Encode);

            UseEncryption = false;
            EncryptionString = "";

            UseCustomTerminatingString = false;
            CustomTerminatingString = "";

            Messages = new List<Message>()
            {
                new Message() { Key=StaticVariables.Message.Text, Value="Text"},
                new Message() { Key=StaticVariables.Message.File, Value="File"}
            };
            SelectedMessage = Messages.Single(m => m.Key == StaticVariables.Message.Text);
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
    }
}
