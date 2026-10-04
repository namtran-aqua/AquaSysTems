using System;
using System.Collections.Generic;
using AquaSolution.Shared.Enum;

namespace AquaSolution.Data.Data.Entities.Feedbacks
{
    public class UserFeedback
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string FullName { get; set; } = string.Empty;
        public int? Age { get; set; }
        public string? Factory { get; set; }
        public string Department { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public FeedbackStatus Status { get; set; } = FeedbackStatus.New;
        public string? AdminNote { get; set; }
        public string? ProcessedBy { get; set; }
        public DateTime? ProcessedAt { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public virtual ICollection<UserFeedbackAttachment> Attachments { get; set; } = new List<UserFeedbackAttachment>();
    }

    public class UserFeedbackAttachment
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid FeedbackId { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string OriginalFileName { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public string? FileType { get; set; }
        public long FileSize { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public virtual UserFeedback? Feedback { get; set; }
    }
}
