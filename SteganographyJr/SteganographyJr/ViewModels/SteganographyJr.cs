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

        public event PropertyChangedEventHandler PropertyChanged;

        public SteganographyJr()
        {
            Modes = new List<Mode>()
            {
                new Mode() { Key=StaticVariables.Modes.Encode, Value="Encode"},
                new Mode() { Key=StaticVariables.Modes.Decode, Value="Decode"}
            };
            SelectedMode = Modes.Single(m => m.Key == StaticVariables.Modes.Encode);
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
    }
}
