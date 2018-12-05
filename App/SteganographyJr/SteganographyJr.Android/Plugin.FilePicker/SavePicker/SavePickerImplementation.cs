using Android.App;
using Android.Content;
using Android.Runtime;
using Java.IO;
using Plugin.FilePicker.Abstractions;
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
        private int _requestId;
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
                throw new InvalidOperationException ("Only one operation can be active at a time");

            try {
                var pickerIntent = new Intent (this._context, typeof (SavePickerActivity));
                pickerIntent.SetFlags (ActivityFlags.NewTask);
                pickerIntent.PutExtra("fileName", fileName);
                pickerIntent.PutExtra("fileBytes", fileBytes);

                this._context.StartActivity (pickerIntent);

                EventHandler<SavePickerEventArgs> handler = null;
                EventHandler<EventArgs> cancelledHandler = null;
                EventHandler<string> errorHandler = null;

                handler = (s, e) => {
                    var tcs = Interlocked.Exchange (ref _completionSource, null);

                    SavePickerActivity.DirectoryPicked -= handler;

                    tcs?.SetResult (null);
                };

                cancelledHandler = (s, e) => {
                    var tcs = Interlocked.Exchange (ref _completionSource, null);

                    SavePickerActivity.DirectoryPickCancelled -= cancelledHandler;

                    tcs?.SetResult (null);
                };

                errorHandler = (s, errorMessage) => {
                    var tcs = Interlocked.Exchange(ref _completionSource, null);

                    SavePickerActivity.DirectoryPickCancelled -= cancelledHandler;

                    tcs?.SetResult(errorMessage);
                };

                SavePickerActivity.DirectoryPickError += errorHandler;
                SavePickerActivity.DirectoryPickCancelled += cancelledHandler;
                SavePickerActivity.DirectoryPicked += handler;
            } catch (Exception exAct) {
                Debug.Write (exAct);
            }

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
    }
}