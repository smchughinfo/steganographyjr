using SteganographyJr.Core.Classes;
using SteganographyJr.Core.ExtensionMethods;
using SteganographyJr.Forms.Interfaces;
using SteganographyJr.Forms.Mvvm;
using System.IO;
using System.Reflection;
using System.Text;
using Xamarin.Forms;

namespace SteganographyJr.Forms.ViewModels
{
    partial class CodecViewModel
    {
        byte[] _carrierImageBytes;
        string _carrierImagePath;   // TODO: android + ios + uwp ....does it make sense to keep track of the path? just keep track of the extension? how difficult to read that from the stream?
        ImageFormat _carrierImageFormat;
        ImageSource _carrierImageSource;
        object _carrierImageNative; // a native representation of the carrier image file, if needed, for the platform to resave.

        bool _changingCarrierImage;
        public DelegateCommand ChangeCarrierImageCommand { get; private set; }

        private void InitCarrierImage()
        {
            var assembly = (typeof(CodecViewModel)).GetTypeInfo().Assembly;

            using (var imageStream = assembly.GetManifestResourceStream(StaticVariables.DefaultCarrierImageResource))
            {
                CarrierImageBytes = imageStream.ConvertToByteArray();
                _carrierImageFormat = StaticVariables.DefaultCarrierImageFormat;
            }

            UpdateCarrierImageSource();

            _changingCarrierImage = false;
            ChangeCarrierImageCommand = new DelegateCommand(
                execute: PickCarrierImage,
                canExecute: () =>
                {
                    return NotExecuting && !ChangingCarrierImage;
                }
            );
        }

        public bool ChangingCarrierImage
        {
            get { return _changingCarrierImage; }
            set { SetPropertyValue(ref _changingCarrierImage, value); }
        }

        private async void PickCarrierImage()
        {
            ChangingCarrierImage = true;

            var imageChooserResult = await DependencyService.Get<IFileIO>().GetFileAsync(true);
            if (imageChooserResult != null)
            {
                var success = string.IsNullOrEmpty(imageChooserResult.ErrorMessage);
                if (success)
                {
                    CarrierImageBytes = imageChooserResult.Stream.ConvertToByteArray();
                    CarrierImagePath = imageChooserResult.Path;
                    _carrierImageNative = imageChooserResult.NativeRepresentation;
                    _carrierImageFormat = imageChooserResult.CarrierImageFormat;

                    imageChooserResult.Stream.Dispose();
                }
                else
                {
                    SendOpenFileErrorMessage(imageChooserResult.ErrorMessage);
                }
            }

            ChangingCarrierImage = false;
        }

        public byte[] CarrierImageBytes
        {
            get { return _carrierImageBytes; }
            set { SetPropertyValue(ref _carrierImageBytes, value); }
        }

        public ImageSource CarrierImageSource
        {
            get { return _carrierImageSource; }
            set
            {
                if (value == null)
                {
                    return;
                }
                SetPropertyValue(ref _carrierImageSource, value);
            }
        }

        private string CarrierImagePath
        {
            get { return _carrierImagePath; }
            set { SetPropertyValue(ref _carrierImagePath, value); }
        }

        private void UpdateCarrierImageSource()
        {
            CarrierImageSource = ImageSource.FromStream(() => new MemoryStream(CarrierImageBytes));
        }

        public string CarrierImageCapacity
        {
            get
            {
                var messageCapacity = _codec.GetImageCapacityInBits(CarrierImageBytes) / 8;
                var size = Utilities.GetHumanReadableFileSize(messageCapacity);
                return $"Message Capacity: {size}";
            }
        }
    }
}
