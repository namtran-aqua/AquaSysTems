using AquaSolution.Data.Data.Entities;
using AquaSolution.Data.Data.Entities.Admin;
using AquaSolution.Data.Data.Entities.KPI;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AquaSolution.Data.Data.MappingConfigurations.KPI
{
    public class KPIApprovalTaskConfiguration : IEntityTypeConfiguration<KPIApprovalTask>
    {
        public void Configure(EntityTypeBuilder<KPIApprovalTask> builder)
        {
            builder.ToTable("tbl_KPIApprovalTasks", schema: "KPI");
            builder.HasKey(e => e.Id);

            builder.HasOne<KPIRequest>()
                     .WithMany()
                     .HasForeignKey(u => u.SubmitId)
                     .OnDelete(DeleteBehavior.Restrict);
            builder.HasOne<User>()
                    .WithMany()
                    .HasForeignKey(u => u.RequesterId)
                    .OnDelete(DeleteBehavior.Restrict);
            builder.HasOne<User>()
                  .WithMany()
                  .HasForeignKey(u => u.ApprovedBy)
                  .OnDelete(DeleteBehavior.Restrict);
            builder.HasOne<User>()
                   .WithMany()
                   .HasForeignKey(u => u.RejectBy)
                   .OnDelete(DeleteBehavior.Restrict);
            builder.Property(e => e.StatusType)
                 .HasConversion<string>()
                 .IsRequired()
                 .HasMaxLength(100);
        }
    }
}
