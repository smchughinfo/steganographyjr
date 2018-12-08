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
using Xamarin.Forms;
using SteganographyJr.Core.ExtensionMethods;
using Plugin.FilePicker.Abstractions;
using SteganographyJr.Forms.DTOs;
using SteganographyJr.Forms;
using SteganographyJr.Forms.Interfaces;
using Plugin.FilePicker;

[assembly: Xamarin.Forms.DependencyAttribute(typeof(SteganographyJr.Droid.Services.DependencyService.FileIO))]
namespace SteganographyJr.Droid.Services.DependencyService
{
    class FileIO : IFileIO
    {
        public async Task<FileChooserResult> GetFileAsync(bool imagesOnly = false)
        {
            try
            {
                var havePermissions = await EnsurePermissions();
                if(havePermissions == false)
                {
                    return new FileChooserResult() { ErrorMessage = StaticVariables.ReadFailedBecauseOfPermissionsMessage };
                }

                var mimeTypesToInclude = imagesOnly ? new string[] { "image/png", "image/jpeg", "image/gif" } : null;
                var result = await CrossFilePicker.Current.PickFile(mimeTypesToInclude);
                
                if(result == null)
                {
                    return null;
                }
                else
                {
                    return new FileChooserResult()
                    {
                        Stream = result.DataArray.ConvertToStream(),
                        FileName = result.FileName
                    };
                }
            }
            catch(Exception ex)
            {
                return new FileChooserResult() { ErrorMessage = ex.Message };
            }
        }

        public async Task<FileSaveResult> SaveFileAsync(string fileName, byte[] fileBytes)
        {
            try
            {
                var savePicker = new Plugin.FilePicker.SavePicker.SavePickerImplementation();
                var errorMessage = await savePicker.SaveFile(fileName, fileBytes);
                var hasError = string.IsNullOrEmpty(errorMessage) == false;
                if(hasError)
                {
                    return new FileSaveResult() { ErrorMessage = errorMessage };
                }
            }
            catch (Exception ex)
            {
                return new FileSaveResult() { ErrorMessage = ex.Message };
            }

            return new FileSaveResult() { ErrorMessage = null };
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
                    await SendPermissionRequestMessage(StaticVariables.RequestPermissionMessage).Task;
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
            MessagingCenter.Subscribe<MainActivity, bool>(this, MainActivity.PermissionsChangedMessageId, (sender, havePermissions) =>
            {
                MessagingCenter.Unsubscribe<MainActivity, bool>(this, MainActivity.PermissionsChangedMessageId);
                promise.SetResult(havePermissions);
            });

            return promise;
        }

        private TaskCompletionSource<bool> SendPermissionRequestMessage(string message)
        {
            var promise = new TaskCompletionSource<bool>();

            var alertMessage = new AlertMessage()
            {
                Title = "Permission Request Explanation",
                CancelButtonText = "Okay",
                Message = message
            };
            MessagingCenter.Send<IFileIO, AlertMessage>(this, StaticVariables.DisplayAlertMessageId, alertMessage);
            MessagingCenter.Subscribe<object>(this, StaticVariables.AlertCompleteMessageId, (sender) =>
            {
                MessagingCenter.Unsubscribe<object>(this, StaticVariables.AlertCompleteMessageId);
                promise.SetResult(true);
            });

            return promise;
        }
    }
}