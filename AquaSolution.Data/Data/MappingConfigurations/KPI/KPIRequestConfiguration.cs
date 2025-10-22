using AquaSolution.Data.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AquaSolution.Data.Data.MappingConfigurations.KPI
{
    public class KPIRequestConfiguration : IEntityTypeConfiguration<KPIRequest>
    {
        public void Configure(EntityTypeBuilder<KPIRequest> builder)
        {
            builder.ToTable("tbl_KPIRequests");
            builder.HasKey(e => e.Id);
 
            builder.HasKey(e => e.SubmitId);
  
            builder.Property(e => e.RequestStatus)
                     .HasConversion<string>()
                     .IsRequired()
                     .HasMaxLength(100);
            builder.HasOne<User>()
                     .WithMany()
                     .HasForeignKey(u => u.ApprovalBy)
                     .OnDelete(DeleteBehavior.Restrict);
            builder.HasOne<User>()
                     .WithMany()
                     .HasForeignKey(u => u.CreatedBy)
                     .OnDelete(DeleteBehavior.Restrict);
            builder.HasOne<User>()
                     .WithMany()
                     .HasForeignKey(u => u.RejectBy)
                     .OnDelete(DeleteBehavior.Restrict);
            builder.Property(d => d.CreatedDate)
                       .IsRequired()
                       .HasDefaultValueSql("GETDATE()");
        }
    }
}
