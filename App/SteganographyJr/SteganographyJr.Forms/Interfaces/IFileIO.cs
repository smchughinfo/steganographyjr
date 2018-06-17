using SteganographyJr.Forms.DTOs;
using System.Threading.Tasks;

namespace SteganographyJr.Forms.Interfaces
{
    public interface IFileIO
    {
        Task<ImageChooserResult> GetFileAsync(bool imagesOnly = false);
        Task<FileSaveResult> SaveImageAsync(string path, byte[] image, object nativeRepresentation = null);
        Task<FileSaveResult> SaveFileAsync(string fileName, byte[] fileBytes);
    }
}