using SteganographyJr.Forms.DTOs;
using System.Threading.Tasks;

namespace SteganographyJr.Forms.Interfaces
{
    public interface IFileIO
    {
        Task<FileChooserResult> GetFileAsync(bool imagesOnly = false);
        Task<FileSaveResult> SaveFileAsync(string fileName, byte[] fileBytes);
    }
}