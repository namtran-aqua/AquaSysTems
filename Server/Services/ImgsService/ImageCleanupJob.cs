using Hangfire;
using System.Threading.Tasks;

namespace AquaSolution.Server.Services.ImgsService
{
    public class ImageCleanupJob
    {
        private readonly IGoogleDriveService _googleDriveService;

        public ImageCleanupJob(IGoogleDriveService googleDriveService)
        {
            _googleDriveService = googleDriveService;
        }

        [AutomaticRetry(Attempts = 3)]
        public async Task ExecuteAsync(string fileId)
        {
            await _googleDriveService.DeleteIfExistsAsync(fileId);
        }
    }
}
