using AquaSolution.Data.Data.Entities.Feedbacks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AquaSolution.Data.Data.MappingConfigurations.Feedbacks
{
    public class UserFeedbackConfiguration : IEntityTypeConfiguration<UserFeedback>
    {
        public void Configure(EntityTypeBuilder<UserFeedback> builder)
        {
            builder.ToTable("tbl_UserFeedbacks");
            builder.HasKey(e => e.Id);
            builder.Property(e => e.SubmissionId).HasMaxLength(100);
            builder.HasIndex(e => e.SubmissionId).IsUnique();

            builder.Property(e => e.FullName).IsRequired().HasMaxLength(250);
            builder.Property(e => e.Department).IsRequired().HasMaxLength(250);
            builder.Property(e => e.PhoneNumber).HasMaxLength(50);
            builder.Property(e => e.Title).IsRequired().HasMaxLength(500);
            builder.Property(e => e.Content).IsRequired();
            builder.Property(e => e.AdminNote).HasMaxLength(2000);
            builder.Property(e => e.ProcessedBy).HasMaxLength(250);

            builder.Property(e => e.FeedbackType).HasMaxLength(250);
            builder.Property(e => e.RelatedParties).HasMaxLength(500);
            builder.Property(e => e.IncidentTimeLocation).HasMaxLength(500);
            builder.Property(e => e.HasFinancialLoss).HasMaxLength(50);
            builder.Property(e => e.EstimatedLossAmount).HasMaxLength(100);
            builder.Property(e => e.HasEvidence).HasMaxLength(50);

            builder.HasMany(e => e.Attachments)
                   .WithOne(a => a.Feedback)
                   .HasForeignKey(a => a.FeedbackId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }

    public class UserFeedbackAttachmentConfiguration : IEntityTypeConfiguration<UserFeedbackAttachment>
    {
        public void Configure(EntityTypeBuilder<UserFeedbackAttachment> builder)
        {
            builder.ToTable("tbl_UserFeedbackAttachments");
            builder.HasKey(e => e.Id);
            builder.Property(e => e.FileName).IsRequired().HasMaxLength(500);
            builder.Property(e => e.OriginalFileName).IsRequired().HasMaxLength(500);
            builder.Property(e => e.FilePath).IsRequired().HasMaxLength(1000);
            builder.Property(e => e.FileType).HasMaxLength(100);

            builder.Property(e => e.GoogleDriveFileId).HasMaxLength(100);
            builder.Property(e => e.DriveUrl).HasMaxLength(1000);
            builder.Property(e => e.SyncStatus).HasMaxLength(50);
            builder.Property(e => e.ErrorMessage).HasMaxLength(1000);
        }
    }

    public class UserFeedbackSyncLogConfiguration : IEntityTypeConfiguration<UserFeedbackSyncLog>
    {
        public void Configure(EntityTypeBuilder<UserFeedbackSyncLog> builder)
        {
            builder.ToTable("tbl_UserFeedbackSyncLogs");
            builder.HasKey(e => e.Id);
            builder.Property(e => e.Status).IsRequired().HasMaxLength(50);
            builder.Property(e => e.TriggerType).IsRequired().HasMaxLength(50);
        }
    }
}
