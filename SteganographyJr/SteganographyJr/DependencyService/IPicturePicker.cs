using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace SteganographyJr.DependencyService
{
    public interface IPicturePicker
    {
        Task<Stream> GetImageStreamAsync();
    }
}