using SteganographyJr.Core;
using SteganographyJr.Core.Classes;
using SteganographyJr.Forms;
using SteganographyJr.Forms.DTOs;
using SteganographyJr.Forms.Interfaces;
using SteganographyJr.Forms.Models;
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
        public async Task<ImageChooserResult> GetFileAsync(bool imagesOnly = false) // TODO: image chooser result?
        {
            try
            {
                // Create and initialize the FileOpenPicker
                FileOpenPicker openPicker = new FileOpenPicker
                {
                    ViewMode = PickerViewMode.Thumbnail
                };

                if (imagesOnly)
                {
                    foreach(var imageType in Core.StaticVariables.ImageFormats)
                    {
                        openPicker.FileTypeFilter.Add(imageType.Extension);
                    }
                    
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

                return new ImageChooserResult()
                {
                    Path = storageFile.Path,
                    Stream = raStream.AsStreamForRead(),
                    NativeRepresentation = storageFile,
                    CarrierImageFormat = new ImageFormat(storageFile.Path)
                };
            }
            catch(Exception ex)
            {
                return new ImageChooserResult() { ErrorMessage = ex.Message };
            }
        }

        public async Task<FileSaveResult> SaveImageAsync(string path, byte[] image, object nativeRepresentation = null)
        {
            try
            {
                StorageFile storageFile;
                if (string.IsNullOrEmpty(path))
                {
                    storageFile = await KnownFolders.PicturesLibrary.CreateFileAsync(Forms.StaticVariables.DefaultCarrierImageSaveName, CreationCollisionOption.ReplaceExisting);
                }
                else
                {
                    storageFile = (StorageFile)nativeRepresentation;
                }
                await Windows.Storage.FileIO.WriteBytesAsync(storageFile, image);

                return new FileSaveResult() { SaveLocation = storageFile.Path };
                //return (true, $"Image saved to {storageFile.Path}.");
            }
            catch(Exception ex)
            {
                return new FileSaveResult() { ErrorMessage = ex.Message };
            }
        }

        // https://docs.microsoft.com/en-us/windows/uwp/files/quickstart-save-a-file-with-a-picker
        public async Task<FileSaveResult> SaveFileAsync(string fileName, byte[] fileBytes)
        {
            try
            {
                FileSavePicker savePicker = new FileSavePicker()
                {
                    SuggestedStartLocation = PickerLocationId.Desktop,
                };

                var ext = Path.GetExtension(fileName);

                savePicker.FileTypeChoices.Add("File", new List<string>() { ext });
                savePicker.SuggestedFileName = fileName;

                StorageFile file = await savePicker.PickSaveFileAsync();
                if (file != null)
                {
                    // Prevent updates to the remote version of the file until
                    // we finish making changes and call CompleteUpdatesAsync.
                    CachedFileManager.DeferUpdates(file);

                    // write to file
                    await Windows.Storage.FileIO.WriteBytesAsync(file, fileBytes);
                    // Let Windows know that we're finished changing the file so
                    // the other app can update the remote version of the file.
                    // Completing updates may require Windows to ask for user input.
                    Windows.Storage.Provider.FileUpdateStatus status = await CachedFileManager.CompleteUpdatesAsync(file);
                    if (status != Windows.Storage.Provider.FileUpdateStatus.Complete)
                    {
                        return new FileSaveResult() { ErrorMessage = "The file couldn't be saved (FileUpdateStatus not complete)." };
                    }
                }

            }
            catch(Exception ex)
            {
                return new FileSaveResult() { ErrorMessage = ex.Message };
            }

            return new FileSaveResult() { ErrorMessage = null };
        }
    }
}