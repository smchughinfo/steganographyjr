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

        public event PropertyChangedEventHandler PropertyChanged;

        public SteganographyJr()
        {
            Modes = new List<Mode>()
            {
                new Mode() { Key=StaticVariables.Modes.Encode, Value="Encode"},
                new Mode() { Key=StaticVariables.Modes.Decode, Value="Decode"}
            };
            SelectedMode = Modes.Single(m => m.Key == StaticVariables.Modes.Encode);

            UseEncryption = false;
            EncryptionString = "";

            UseCustomTerminatingString = false;
            CustomTerminatingString = "";
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
    }
}
