using SteganographyJr.Models;
using SteganographyJr.Services.DependencyService;
using SteganographyJr.UWP.Services.DependencyService;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;

[assembly: Xamarin.Forms.DependencyAttribute(typeof(SteganographyJr.UWP.Services.DependencyService.FileIO))]
namespace SteganographyJr.UWP.Services.DependencyService
{
    public class FileIO : IFileIO
    {
        // https://docs.microsoft.com/en-us/xamarin/xamarin-forms/app-fundamentals/dependency-service/photo-picker
        public async Task<StreamWithPath> GetStreamWithPathAsync(bool imagesOnly = false)
        {
            // Create and initialize the FileOpenPicker
            FileOpenPicker openPicker = new FileOpenPicker
            {
                ViewMode = PickerViewMode.Thumbnail
            };

            if(imagesOnly)
            {
                openPicker.FileTypeFilter.Add(".jpg");
                openPicker.FileTypeFilter.Add(".jpeg");
                openPicker.FileTypeFilter.Add(".png");
                openPicker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
            }
            else
            {
                openPicker.FileTypeFilter.Add("*");
                openPicker.SuggestedStartLocation = PickerLocationId.ComputerFolder;
            }

            // Get a file and return a Stream
            StorageFile storageFile = await openPicker.PickSingleFileAsync();

            if (storageFile == null)
            {
                return null;
            }
            
            IRandomAccessStreamWithContentType raStream = await storageFile.OpenReadAsync();

            return new StreamWithPath()
            {
                Path = storageFile.Path,
                Stream = raStream.AsStreamForRead()
            };
        }

        // https://docs.microsoft.com/en-us/windows/uwp/files/quickstart-save-a-file-with-a-picker
        public async Task<bool> SaveImage(StreamWithPath carrierImage)
        {
            string fileName = Path.GetFileName(carrierImage.Path);
            string fileExtension = Path.GetExtension(carrierImage.Path);

            FileSavePicker savePicker = new FileSavePicker()
            {
                SuggestedStartLocation = PickerLocationId.PicturesLibrary,
            };

            savePicker.FileTypeChoices.Add("Images", new List<string>() { fileExtension });
            savePicker.SuggestedFileName = fileName;

            StorageFile file = await savePicker.PickSaveFileAsync();
            if (file != null)
            {
                // Prevent updates to the remote version of the file until
                // we finish making changes and call CompleteUpdatesAsync.
                CachedFileManager.DeferUpdates(file);

                // write to file
                await Windows.Storage.FileIO.WriteBytesAsync(file, carrierImage.GetBytes());
                // Let Windows know that we're finished changing the file so
                // the other app can update the remote version of the file.
                // Completing updates may require Windows to ask for user input.
                Windows.Storage.Provider.FileUpdateStatus status = await CachedFileManager.CompleteUpdatesAsync(file);
                if (status != Windows.Storage.Provider.FileUpdateStatus.Complete)
                {
                    return false;
                }
            }

            return true;
        }
    }
}