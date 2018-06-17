using SteganographyJr.Forms.Models;
using System.Collections.Generic;
using System.Linq;

namespace SteganographyJr.Forms.ViewModels
{
    // mode and password have nothing to do with each other. theyre grouped together because they dont have a lot of code.
    partial class CodecViewModel
    {
        List<ModeType> _modes;
        ModeType _selectedMode;

        bool _usePassword;
        string _password;

        private void InitMode()
        {
            ModeTypes = new List<ModeType>()
            {
                new ModeType() { Key=StaticVariables.Mode.Encode, Value="Encode" },
                new ModeType() { Key=StaticVariables.Mode.Decode, Value="Decode" }
            };
            SelectedMode = ModeTypes.Single(m => m.Key == StaticVariables.Mode.Encode);
        }

        private void InitPassword()
        {
            UsePassword = false;
            Password = "";
        }

        public List<ModeType> ModeTypes
        {
            get { return _modes; }
            set { SetPropertyValue(ref _modes, value); }
        }

        public ModeType SelectedMode
        {
            get { return _selectedMode; }
            set { SetPropertyValue(ref _selectedMode, value); }
        }

        public bool SelectedModeIsEncode
        {
            get { return SelectedMode.Key == StaticVariables.Mode.Encode; }
        }

        public bool SelectedModeIsDecode
        {
            get { return SelectedMode.Key == StaticVariables.Mode.Decode; }
        }

        public bool UsePassword
        {
            get { return _usePassword; }
            set { SetPropertyValue(ref _usePassword, value); }
        }

        public bool EnablePassword
        {
            get { return UsePassword && NotExecuting; }
        }

        public bool DisablePassword
        {
            get { return !EnablePassword; }
        }

        public string Password
        {
            get { return _password; }
            set { SetPropertyValue(ref _password, value); }
        }
    }
}
