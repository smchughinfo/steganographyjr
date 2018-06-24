using System;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using System.Threading.Tasks;
using Plugin.FilePicker.Abstractions;
using Android.Provider;
using System.Net;
using Java.IO;
using System.IO;

namespace SteganographyJr.Droid.Plugin.FilePicker.SavePicker
{
    [Activity (ConfigurationChanges = Android.Content.PM.ConfigChanges.Orientation | Android.Content.PM.ConfigChanges.ScreenSize)]
    [Preserve (AllMembers = true)]
    public class SavePickerActivity : Activity
    {
        string fileName;
        byte[] fileBytes;

        protected override void OnCreate (Bundle savedInstanceState)
        {
            base.OnCreate (savedInstanceState);

            var fileNameWithExtension = this.Intent.GetStringExtra("fileName");
            var fileExtension = Path.GetExtension(fileNameWithExtension);
            var mimeType = Android.Webkit.MimeTypeMap.Singleton.GetMimeTypeFromExtension(fileExtension.Substring(1));

            fileName = fileNameWithExtension.Replace(fileExtension, "");
            fileBytes = Intent.GetByteArrayExtra("fileBytes");

            var intent = new Intent(Intent.ActionCreateDocument);
            intent.AddCategory(Intent.CategoryOpenable);
            intent.SetType(mimeType);
            intent.PutExtra(Intent.ExtraTitle, fileNameWithExtension);
            StartActivityForResult(Intent.CreateChooser(intent, "Select Save Location"), 43);
        }

        protected override void OnActivityResult (int requestCode, [GeneratedEnum] Result resultCode, Intent data)
        {
            base.OnActivityResult (requestCode, resultCode, data);

            if (resultCode == Result.Canceled && requestCode == 43) {
                // Notify user file picking was cancelled.
                OnDirectoryPickCancelled ();
                Finish ();
            } else {
                System.Diagnostics.Debug.Write (data.Data);
                try {
                    using (var pFD = ContentResolver.OpenFileDescriptor(data.Data, "w"))
                    using (var outputSteam = new FileOutputStream(pFD.FileDescriptor))
                    {
                        outputSteam.Write(fileBytes);
                    }
                    
                    OnDirectoryPicked (new SavePickerEventArgs (data.Data));
                } catch (Exception readEx) {
                    // Notify user file picking failed.
                    OnDirectoryPickError(readEx.Message);
                    System.Diagnostics.Debug.Write (readEx);
                } finally {
                    Finish ();
                }
            }
        }

        internal static event EventHandler<SavePickerEventArgs> DirectoryPicked;
        internal static event EventHandler<EventArgs> DirectoryPickCancelled;
        internal static event EventHandler<string> DirectoryPickError;

        private static void OnDirectoryPickCancelled ()
        {
            DirectoryPickCancelled?.Invoke (null, null);
        }

        private static void OnDirectoryPickError(string message)
        {
            DirectoryPickError?.Invoke(null, message);
        }

        private static void OnDirectoryPicked (SavePickerEventArgs e)
        {
            DirectoryPicked?.Invoke(null, e);
        }
    }
}