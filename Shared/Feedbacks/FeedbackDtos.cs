using System;
using System.Collections.Generic;
using AquaSolution.Shared.Enum;

namespace AquaSolution.Shared.Feedbacks
{
    public class UserFeedbackCreateDto
    {
        public string? SubmissionId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public int? Age { get; set; }
        public string Factory { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;

        public string? FeedbackType { get; set; }
        public string? IssueGroups { get; set; }
        public string? RelatedParties { get; set; }
        public string? IncidentTimeLocation { get; set; }
        public string? HasFinancialLoss { get; set; }
        public string? EstimatedLossAmount { get; set; }
        public string? HasEvidence { get; set; }
        public string? EvidenceDescription { get; set; }

        public List<UserFeedbackAttachmentCreateDto> Attachments { get; set; } = new();
    }

    public class UserFeedbackAttachmentCreateDto
    {
        public string FileName { get; set; } = string.Empty;
        public string OriginalFileName { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public string FileType { get; set; } = string.Empty;
        public long FileSize { get; set; }
        public string? GoogleDriveFileId { get; set; }
        public string? DriveUrl { get; set; }
    }

    public class UserFeedbackAttachmentDto
    {
        public Guid Id { get; set; }
        public Guid FeedbackId { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string OriginalFileName { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public string FileType { get; set; } = string.Empty;
        public long FileSize { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? GoogleDriveFileId { get; set; }
        public string? DriveUrl { get; set; }
        public string? SyncStatus { get; set; }
        public string? ErrorMessage { get; set; }
    }

    public class UserFeedbackDto
    {
        public Guid Id { get; set; }
        public string? SubmissionId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public int? Age { get; set; }
        public string Factory { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public FeedbackStatus Status { get; set; } = FeedbackStatus.New;
        public string? AdminNote { get; set; }
        public string? ProcessedBy { get; set; }
        public DateTime? ProcessedAt { get; set; }
        public DateTime CreatedAt { get; set; }

        public string? FeedbackType { get; set; }
        public string? IssueGroups { get; set; }
        public List<string> ParsedIssueGroups { get; set; } = new();
        public string? RelatedParties { get; set; }
        public string? IncidentTimeLocation { get; set; }
        public string? HasFinancialLoss { get; set; }
        public string? EstimatedLossAmount { get; set; }
        public string? HasEvidence { get; set; }
        public string? EvidenceDescription { get; set; }

        public List<UserFeedbackAttachmentDto> Attachments { get; set; } = new();
    }

    public class UserFeedbackFilterDto
    {
        public string? SearchTerm { get; set; }
        public string? Factory { get; set; }
        public string? Department { get; set; }
        public FeedbackStatus? Status { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public int PageIndex { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

    public class UserFeedbackStatusUpdateDto
    {
        public Guid Id { get; set; }
        public FeedbackStatus Status { get; set; }
        public string? AdminNote { get; set; }
    }

    public class FeedbackPagedResult<T>
    {
        public List<T> Items { get; set; } = new();
        public int TotalCount { get; set; }
        public int PageIndex { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => PageSize > 0 ? (int)Math.Ceiling((double)TotalCount / PageSize) : 0;
    }

    public class FeedbackDashboardStatsDto
    {
        public int TotalCount { get; set; }
        public int NewCount { get; set; }
        public int ProcessingCount { get; set; }
        public int ResolvedCount { get; set; }
        public int ClosedCount { get; set; }
        public int AnonymousCount { get; set; }
        public int ConfidentialCount { get; set; }
        public int EvidenceCount { get; set; }
    }

    public class FeedbackSyncResultDto
    {
        public bool Success { get; set; }
        public int TotalRows { get; set; }
        public int NewCount { get; set; }
        public int SkippedCount { get; set; }
        public int FailedCount { get; set; }
        public int AttachmentCount { get; set; }
        public string Message { get; set; } = string.Empty;
        public DateTime? LastSyncTime { get; set; }
    }

    public class FeedbackSyncStatusDto
    {
        public DateTime? LastSyncTime { get; set; }
        public string Status { get; set; } = "None";
        public int TotalRows { get; set; }
        public int NewRows { get; set; }
        public int SkippedRows { get; set; }
        public int FailedRows { get; set; }
        public int AttachmentCount { get; set; }
        public string TriggerType { get; set; } = string.Empty;
        public string? ErrorMessage { get; set; }
    }
}
