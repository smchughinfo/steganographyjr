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

        public void SaveImage(Stream image, string path)
        {

        }
    }
}