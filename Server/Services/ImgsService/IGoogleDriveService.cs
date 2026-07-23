using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace AquaSolution.Server.Services.ImgsService
{
    public interface IGoogleDriveService
    {
        Task DeleteOldFilesAsync(int daysOld);
    }
}
