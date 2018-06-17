using SteganographyJr.Forms.DTOs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace SteganographyJr.Forms.Services.DependencyService
{
    public interface IFileIO
    {
        Task<ImageChooserResult> GetFileAsync(bool imagesOnly = false);
        Task<FileSaveResult> SaveImageAsync(string path, byte[] image, object nativeRepresentation = null);
        Task<FileSaveResult> SaveFileAsync(string fileName, byte[] fileBytes);
    }
}