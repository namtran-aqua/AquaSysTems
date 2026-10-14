using System;

namespace AquaSolution.Data.Data.Entities.Feedbacks
{
    public class UserFeedbackSyncLog
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public DateTime StartedAt { get; set; } = DateTime.Now;
        public DateTime? CompletedAt { get; set; }
        public string Status { get; set; } = "Running";
        public int TotalRows { get; set; }
        public int NewRows { get; set; }
        public int SkippedRows { get; set; }
        public int FailedRows { get; set; }
        public int AttachmentCount { get; set; }
        public string TriggerType { get; set; } = "Manual"; // Manual / Hangfire
        public string? ErrorMessage { get; set; }
    }
}
