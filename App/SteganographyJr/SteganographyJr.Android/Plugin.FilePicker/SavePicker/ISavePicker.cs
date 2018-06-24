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
using Plugin.FilePicker.Abstractions;

namespace SteganographyJr.Droid.Plugin.FilePicker.SavePicker
{
    public interface ISavePicker
    {
        Task<string> SaveFile(string fileName, byte[] fileBytes);
    }
}