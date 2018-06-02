using SteganographyJr.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace SteganographyJr.Services.DependencyService
{
    public interface IFileIO
    {
        Task<StreamWithPath> GetStreamWithPathAsync(bool imagesOnly = false);
        void SaveImage(Stream image, string path);
    }
}