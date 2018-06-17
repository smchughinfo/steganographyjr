using SteganographyJr.Core.ExtensionMethods;
using SteganographyJr.Forms.Interfaces;
using SteganographyJr.Forms.Models;
using SteganographyJr.Forms.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using Xamarin.Forms;

namespace SteganographyJr.Forms.ViewModels
{
    partial class CodecViewModel
    {
        List<MessageType> _messages;      // text or file
        MessageType _SelectedMessageType; // text or file, whichever is selected
        string _textMessage;              // if text is selected, the text the user entered
        BytesWithPath _fileMessage;       // if file is selected, the file the user selected

        bool _changingMessageFile;
        public DelegateCommand ChangeMessageFileCommand { get; private set; }

        private void InitMessage()
        {
            MessageTypes = new List<MessageType>()
            {
                new MessageType() { Key=StaticVariables.Message.Text, Value="Text" },
                new MessageType() { Key=StaticVariables.Message.File, Value="File" }
            };

            TextMessage = "Type your message here";

            SelectedMessageType = MessageTypes.Single(m => m.Key == StaticVariables.Message.Text);

            _changingMessageFile = false;
            ChangeMessageFileCommand = new DelegateCommand(
                execute: PickFileMessage,
                canExecute: () =>
                {
                    return NotExecuting && !ChangingMessageFile;
                }
            );
        }

        private async void PickFileMessage()
        {
            ChangingMessageFile = true;

            var imageChooserResult = await DependencyService.Get<IFileIO>().GetFileAsync();
            if (imageChooserResult != null)
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

        public bool ChangingMessageFile
        {
            get { return _changingMessageFile; }
            set { SetPropertyValue(ref _changingMessageFile, value); }
        }

        public List<MessageType> MessageTypes
        {
            get { return _messages; }
            set { SetPropertyValue(ref _messages, value); }
        }

        public MessageType SelectedMessageType
        {
            get { return _SelectedMessageType; }
            set { SetPropertyValue(ref _SelectedMessageType, value); }
        }

        public string TextMessage
        {
            get { return _textMessage; }
            set { SetPropertyValue(ref _textMessage, value); }
        }

        public BytesWithPath FileMessage
        {
            get { return _fileMessage; }
            set { SetPropertyValue(ref _fileMessage, value); }
        }

        public bool UsingTextMessage
        {
            get { return SelectedMessageType.Key == StaticVariables.Message.Text; }
        }

        public bool ShowTextMessage
        {
            get { return SelectedModeIsEncode && UsingTextMessage; }
        }

        public bool UsingFileMessage
        {
            get { return SelectedMessageType.Key == StaticVariables.Message.File; }
        }

        public bool ShowFileMessage
        {
            get { return SelectedModeIsEncode && UsingFileMessage; }
        }
    }
}
