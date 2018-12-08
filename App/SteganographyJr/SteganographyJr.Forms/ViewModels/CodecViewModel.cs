using SteganographyJr.Forms.Interfaces;
using SteganographyJr.Forms.Mvvm;

namespace SteganographyJr.Forms.ViewModels
{
    partial class CodecViewModel : ObservableObject, IViewModel
    {
        public CodecViewModel()
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
                .AlsoInvokeAction(ExecuteCommand.ChangeCanExecute)
                .AlsoRaisePropertyChangedFor(() => NotExecuting)
                .AlsoRaisePropertyChangedFor(() => ExecutionMessage);

            WhenPropertyChanges(() => NotExecuting)
                .AlsoRaisePropertyChangedFor(() => EnablePassword);

            WhenPropertyChanges(() => CarrierImageBytes)
                .AlsoInvokeAction(UpdateCarrierImageSource)
                .AlsoRaisePropertyChangedFor(() => CarrierImageCapacity);

            WhenPropertyChanges(() => SelectedMode)
                .AlsoInvokeAction(ExecuteCommand.ChangeCanExecute)
                .AlsoRaisePropertyChangedFor(() => SelectedModeIsEncode)
                .AlsoRaisePropertyChangedFor(() => SelectedModeIsDecode)
                .AlsoRaisePropertyChangedFor(() => ShowTextMessage)
                .AlsoRaisePropertyChangedFor(() => ShowFileMessage);

            WhenPropertyChanges(() => UsePassword)
                .AlsoInvokeAction(ExecuteCommand.ChangeCanExecute)
                .AlsoRaisePropertyChangedFor(() => EnablePassword)
                .AlsoRaisePropertyChangedFor(() => DisablePassword);

            WhenPropertyChanges(() => SelectedMessageType)
                .AlsoInvokeAction(ExecuteCommand.ChangeCanExecute)
                .AlsoRaisePropertyChangedFor(() => UsingTextMessage)
                .AlsoRaisePropertyChangedFor(() => ShowTextMessage)
                .AlsoRaisePropertyChangedFor(() => UsingFileMessage)
                .AlsoRaisePropertyChangedFor(() => ShowFileMessage);

            WhenPropertyChanges(() => CarrierImagePath)
                .AlsoRaisePropertyChangedFor(() => CarrierImageFileName);

            WhenPropertyChanges(() => Password)
                .AlsoInvokeAction(ExecuteCommand.ChangeCanExecute);

            WhenPropertyChanges(() => TextMessage)
                .AlsoInvokeAction(ExecuteCommand.ChangeCanExecute);

            WhenPropertyChanges(() => FileMessage)
                .AlsoInvokeAction(ExecuteCommand.ChangeCanExecute);
        }
    }
}
