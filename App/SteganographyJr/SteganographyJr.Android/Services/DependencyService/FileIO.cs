using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using SteganographyJr.DTOs;
using SteganographyJr.Services.DependencyService;

[assembly: Xamarin.Forms.DependencyAttribute(typeof(SteganographyJr.Droid.Services.DependencyService.FileIO))]
namespace SteganographyJr.Droid.Services.DependencyService
{
    // TODO: add the code in MainActivity \n -> 
    // https://docs.microsoft.com/en-us/xamarin/xamarin-forms/app-fundamentals/dependency-service/photo-picker
    class FileIO : IFileIO
    {
        public Task<ImageChooserResult> GetFileAsync(bool imagesOnly = false)
        {
            return null;
        }

        public Task<FileSaveResult> SaveImageAsync(string path, byte[] image, object nativeRepresentation = null)
        {
            return null;
        }

        public Task<FileSaveResult> SaveFileAsync(string fileName, byte[] fileBytes)
        {
            return null;
        }
    }
}