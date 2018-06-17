using SteganographyJr.Forms.DTOs;
using SteganographyJr.Forms.Interfaces;
using SteganographyJr.Forms.Mvvm;
using Xamarin.Forms;

namespace SteganographyJr.Forms.ViewModels
{
    partial class CodecViewModel : ObservableObject, IViewModel
    {
        private void SendErrorMessage(string errorMessage)
        {
            var alertMessage = new AlertMessage()
            {
                Title = "Error",
                CancelButtonText = "Okay",
                Message = errorMessage
            };
            MessagingCenter.Send<IViewModel, AlertMessage>(this, StaticVariables.DisplayAlertMessageId, alertMessage);
        }

        private void SendDecodedMessage(string decodedMessage)
        {
            var alertMessage = new AlertMessage()
            {
                Title = "Message Decoded",
                CancelButtonText = "Okay",
                Message = $"Decoded message: {decodedMessage}"
            };
            MessagingCenter.Send<IViewModel, AlertMessage>(this, StaticVariables.DisplayAlertMessageId, alertMessage);
        }

        private void SendEncodingSuccessMessage(string imagePath)
        {
            var alertMessage = new AlertMessage()
            {
                Title = "Message Encoded",
                CancelButtonText = "Excelsior!",
                Message = $"Image saved to {imagePath}."
            };
            MessagingCenter.Send<IViewModel, AlertMessage>(this, StaticVariables.DisplayAlertMessageId, alertMessage);
        }

        private void SendEncodingErrorMessage(string errorMessage)
        {
            var alertMessage = new AlertMessage()
            {
                Title = "Encoding Error",
                CancelButtonText = "Okay",
                Message = errorMessage
            };
            MessagingCenter.Send<IViewModel, AlertMessage>(this, StaticVariables.DisplayAlertMessageId, alertMessage);
        }

        private void SendOpenFileErrorMessage(string errorMessage)
        {
            var alertMessage = new AlertMessage()
            {
                Title = "Open File Error",
                CancelButtonText = "Okay",
                Message = errorMessage
            };
            MessagingCenter.Send<IViewModel, AlertMessage>(this, StaticVariables.DisplayAlertMessageId, alertMessage);
        }
    }
}
