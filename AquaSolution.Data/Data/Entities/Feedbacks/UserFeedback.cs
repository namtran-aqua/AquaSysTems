using System;
using System.Collections.Generic;
using AquaSolution.Shared.Enum;

namespace AquaSolution.Data.Data.Entities.Feedbacks
{
    public class UserFeedback
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string? SubmissionId { get; set; }
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

        // Các trường từ Google Form
        public string? FeedbackType { get; set; }
        public string? IssueGroups { get; set; }
        public string? RelatedParties { get; set; }
        public string? IncidentTimeLocation { get; set; }
        public string? HasFinancialLoss { get; set; }
        public string? EstimatedLossAmount { get; set; }
        public string? HasEvidence { get; set; }
        public string? EvidenceDescription { get; set; }

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

        // Google Drive integration
        public string? GoogleDriveFileId { get; set; }
        public string? DriveUrl { get; set; }
        public string? SyncStatus { get; set; }
        public string? ErrorMessage { get; set; }

        public virtual UserFeedback? Feedback { get; set; }
    }
}
