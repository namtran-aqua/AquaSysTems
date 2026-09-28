using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using System.Collections.Generic;
using AquaSolution.Shared.Imgs;

namespace AquaSolution.Server.Services.ImgsService
{
    public interface IGoogleDriveService
    {
        Task DeleteOldFilesAsync(int daysOld);
        Task<Dictionary<string, string>> GetFoldersAsync();
        Task<List<GoogleDriveImageDto>> GetAllImagesAsync(List<string> allowedFolderNames);
        Task<byte[]> GetFileBytesAsync(string fileId);
        Task<bool> DeleteFileAsync(string fileId);
    }
}
