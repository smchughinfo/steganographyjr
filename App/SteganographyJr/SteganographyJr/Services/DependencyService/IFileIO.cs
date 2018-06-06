using SteganographyJr.DTOs;
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
        Task<ImageChooserResult> GetFileAsync(bool imagesOnly = false);
        Task<ImageSaveResult> SaveImage(string path, byte[] image, object nativeRepresentation = null);
    }
}