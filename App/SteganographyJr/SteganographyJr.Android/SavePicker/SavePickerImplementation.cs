using Android.App;
using Android.Content;
using Android.Runtime;
using Java.IO;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;

namespace SteganographyJr.Droid.Plugin.FilePicker.SavePicker
{
    /// <summary>
    /// Implementation for Feature
    /// </summary>
    ///
    [Preserve (AllMembers = true)]
    public class SavePickerImplementation : ISavePicker
    {
        private readonly Context _context;
        private int _requestId = 0;
        private TaskCompletionSource<string> _completionSource;

        public SavePickerImplementation ()
        {
            _context = Application.Context;
        }
        
        public Task<string> SaveFile(string fileName, byte[] fileBytes)
        {
            var id = GetRequestId ();

            var ntcs = new TaskCompletionSource<string> (id);

            if (Interlocked.CompareExchange (ref _completionSource, ntcs, null) != null)
                throw new InvalidOperationException("Only one operation can be active at a time");

            Task.Run(async () => {
                try
                {
                    var pathToFile = await WriteFileToCacheDirectory(fileName, fileBytes);

                    var pickerIntent = new Intent(this._context, typeof(SavePickerActivity));
                    pickerIntent.SetFlags(ActivityFlags.NewTask);
                    pickerIntent.PutExtra("fileName", fileName);
                    pickerIntent.PutExtra("path", pathToFile);

                    this._context.StartActivity(pickerIntent);

                    EventHandler<SavePickerEventArgs> handler = null;
                    EventHandler<EventArgs> cancelledHandler = null;
                    EventHandler<string> errorHandler = null;

                    handler = (s, e) => {
                        var tcs = Interlocked.Exchange(ref _completionSource, null);

                        SavePickerActivity.DirectoryPicked -= handler;

                        tcs?.SetResult(null);
                    };

                    cancelledHandler = (s, e) => {
                        var tcs = Interlocked.Exchange(ref _completionSource, null);

                        SavePickerActivity.DirectoryPickCancelled -= cancelledHandler;

                        tcs?.SetResult(null);
                    };

                    errorHandler = (s, errorMessage) => {
                        var tcs = Interlocked.Exchange(ref _completionSource, null);

                        SavePickerActivity.DirectoryPickCancelled -= cancelledHandler;

                        tcs?.SetResult(errorMessage);
                    };

                    SavePickerActivity.DirectoryPickError += errorHandler;
                    SavePickerActivity.DirectoryPickCancelled += cancelledHandler;
                    SavePickerActivity.DirectoryPicked += handler;
                }
                catch (Exception exAct)
                {
                    // TODO: notify user 
                    Debug.Write(exAct);
                }
            });

            return _completionSource.Task;
        }

        private int GetRequestId ()
        {
            int id = _requestId;

            if (_requestId == int.MaxValue)
                _requestId = 0;
            else
                _requestId++;

            return id;
        }

        private async Task<string> WriteFileToCacheDirectory(string fileName, byte[] fileBytes)
        {
            File outputDir = this._context.CacheDir;
            File file = new File(outputDir, fileName);

            FileOutputStream fos = new FileOutputStream(file.AbsolutePath);
            await fos.WriteAsync(fileBytes);
            fos.Flush();
            fos.Close();
            return file.AbsolutePath;
        }
    }
}