using SteganographyJr.DTOs;
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
        public async Task<ImageChooserResult> GetFileAsync(bool imagesOnly = false)
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

                return new ImageChooserResult()
                {
                    Path = storageFile.Path,
                    Stream = raStream.AsStreamForRead(),
                    NativeRepresentation = storageFile
                };
            }
            catch(Exception ex)
            {
                return new ImageChooserResult() { ErrorMessage = ex.Message };
            }
        }

        public async Task<ImageSaveResult> SaveImage(string path, byte[] image, object nativeRepresentation = null)
        {
            try
            {
                StorageFile storageFile;
                if (string.IsNullOrEmpty(path))
                {
                    storageFile = await KnownFolders.PicturesLibrary.CreateFileAsync(StaticVariables.defaultCarrierImageSaveName, CreationCollisionOption.ReplaceExisting);
                }
                else
                {
                    storageFile = (StorageFile)nativeRepresentation;
                }
                await Windows.Storage.FileIO.WriteBytesAsync(storageFile, image);

                return new ImageSaveResult() { SaveLocation = storageFile.Path };
                //return (true, $"Image saved to {storageFile.Path}.");
            }
            catch(Exception ex)
            {
                return new ImageSaveResult() { ErrorMessage = ex.Message };
            }
        }
    }
}