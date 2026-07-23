using System;

namespace AquaSolution.Server.Services.ImgsService
{
    public interface IImageCleanupScheduler
    {
        string ScheduleCleanup(string fileId, TimeSpan delay);
    }
}
