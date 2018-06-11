using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Android;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Provider;
using Android.Runtime;
using Android.Support.Design.Widget;
using Android.Support.V4.App;
using Android.Support.V4.Content;
using Android.Views;
using Android.Widget;
using SteganographyJr.DTOs;
using SteganographyJr.Services.DependencyService;
using Xamarin.Forms;

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

        // https://docs.microsoft.com/en-us/xamarin/android/app-fundamentals/permissions?tabs=vswin
        public async Task<FileSaveResult> SaveImageAsync(string path, byte[] image, object nativeRepresentation = null)
        {
            try
            {
                await EnsurePermissions();
                if (string.IsNullOrEmpty(path))
                {
                    Bitmap bitmap = new Bitmap();
                    bitmap.Set(image);
                    path = MediaStore.Images.Media.InsertImage(MainActivity.Instance.ContentResolver, bitmap.platformBitmap, StaticVariables.DefaultCarrierImageSaveName, null);
                }
                else
                {
                    // TODO: this probably doesn't work
                    File.WriteAllBytes(path, image);
                }

                return new FileSaveResult() { SaveLocation = path };
            }
            catch (Exception ex)
            {
                return null;//new FileSaveResult() { ErrorMessage = ex.Message };
            }
        }

        public Task<FileSaveResult> SaveFileAsync(string fileName, byte[] fileBytes)
        {
            return null;
        }

        private async Task<bool> EnsurePermissions()
        {
            string[] requiredPermissions = new string[] {
                Manifest.Permission.ReadExternalStorage,
                Manifest.Permission.WriteExternalStorage
            };

            var needPermission = requiredPermissions.Any(permission =>
            {
                return ContextCompat.CheckSelfPermission(Android.App.Application.Context, permission) != (int)Permission.Granted;
            });
                

            if (needPermission)
            {
                var showPermissionExplanation = requiredPermissions.Any(permission =>
                {
                    return ActivityCompat.ShouldShowRequestPermissionRationale(MainActivity.Instance, permission);
                });

                if(showPermissionExplanation)
                {
                    SendPermissionRequestMessage(StaticVariables.RequestPermissionMessage);
                }
                
                ActivityCompat.RequestPermissions(MainActivity.Instance, requiredPermissions, 0);
                return await PermissionNotification().Task;
            }

            return true;
        }

        private TaskCompletionSource<bool> PermissionNotification()
        {
            TaskCompletionSource<bool> promise = new TaskCompletionSource<bool>();

            // TODO: possible extra subscription here if user denies first request?
            MessagingCenter.Subscribe<MainActivity, bool>(this, MainActivity.PermissionsChangedMessage, (sender, havePermissions) =>
            {
                MessagingCenter.Unsubscribe<MainActivity, bool>(this, MainActivity.PermissionsChangedMessage);
                promise.SetResult(havePermissions);
            });

            return promise;
        }

        private void SendPermissionRequestMessage(string message)
        {
            var alertMessage = new AlertMessage()
            {
                Title = "Permission Request Explanation",
                CancelButtonText = "Okay",
                Message = message
            };
            MessagingCenter.Send<IFileIO, AlertMessage>(this, StaticVariables.DisplayAlertMessage, alertMessage);
        }
    }
}