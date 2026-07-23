using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace AquaSolution.Server.Services.ImgsService
{
    public interface IGoogleDriveService
    {
        Task<(string url, string fileId)> UploadAsync(IFormFile file, string workDayId);
        Task DeleteIfExistsAsync(string fileId);
    }
}
