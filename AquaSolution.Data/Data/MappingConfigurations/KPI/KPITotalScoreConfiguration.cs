using AquaSolution.Data.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AquaSolution.Data.Data.MappingConfigurations.KPI
{
    public class KPITotalScoreConfiguration : IEntityTypeConfiguration<KPITotalScore>
    {
        public void Configure(EntityTypeBuilder<KPITotalScore> builder)
        {
            builder.ToTable("tbl_KPITotalScores");
            builder.HasKey(e => e.Id);
  
            builder.HasOne<KPIRequest>()
                     .WithMany()
                     .HasForeignKey(u => u.SubmitId)
                     .OnDelete(DeleteBehavior.Restrict);
                        builder.Property(e => e.Status)
                      .HasConversion<string>()
                      .IsRequired()
                      .HasMaxLength(100);
            builder.Property(d => d.CreatedDate)
                       .IsRequired()
                       .HasDefaultValueSql("GETDATE()");
        }
    }
}
