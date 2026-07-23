using System.Threading.Tasks;

namespace AquaSolution.Server.Services.ImgsService
{
    public class DailyImageCleanupJob
    {
        private readonly IGoogleDriveService _googleDriveService;

        public DailyImageCleanupJob(IGoogleDriveService googleDriveService)
        {
            _googleDriveService = googleDriveService;
        }

        public async Task ExecuteAsync()
        {
            // Xóa tất cả các file cũ hơn 2 ngày
            await _googleDriveService.DeleteOldFilesAsync(2);
        }
    }
}
