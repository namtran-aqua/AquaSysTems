using Hangfire;
using System;

namespace AquaSolution.Server.Services.ImgsService
{
    public class ImageCleanupScheduler : IImageCleanupScheduler
    {
        public string ScheduleCleanup(string fileId, TimeSpan delay)
        {
            return BackgroundJob.Schedule<ImageCleanupJob>(
                job => job.ExecuteAsync(fileId),
                delay);
        }
    }
}
