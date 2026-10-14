using System;
using System.Threading;
using System.Threading.Tasks;
using AquaSolution.Shared.Feedbacks;

namespace AquaSolution.Server.Services.Feedbacks
{
    public interface IFeedbackSyncService
    {
        Task<FeedbackSyncResultDto> SyncAsync(string triggerType = "Manual", CancellationToken cancellationToken = default);
        Task<FeedbackSyncStatusDto> GetSyncStatusAsync();
    }
}
