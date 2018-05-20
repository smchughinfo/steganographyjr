using SteganographyJr.Services.DependencyService;
using SteganographyJr.Models;
using SteganographyJr.UWP.DependencyService;
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

[assembly: Xamarin.Forms.Dependency(typeof(FilePickerImplementation))]

namespace SteganographyJr.UWP.DependencyService
{
    public class FilePickerImplementation : IFilePicker
    {
        public async Task<StreamWithPath> GetStreamWithPathAsync()
        {
            // Create and initialize the FileOpenPicker
            FileOpenPicker openPicker = new FileOpenPicker
            {
                ViewMode = PickerViewMode.Thumbnail,
                SuggestedStartLocation = PickerLocationId.ComputerFolder
            };

            openPicker.FileTypeFilter.Add("*");

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
    }
}