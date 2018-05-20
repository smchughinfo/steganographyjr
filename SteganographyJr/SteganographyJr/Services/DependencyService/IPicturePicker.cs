using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace SteganographyJr.Services.DependencyService
{
    public interface IPicturePicker
    {
        Task<Stream> GetImageStreamAsync();
    }
}