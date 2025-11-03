using AquaSolution.Data.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AquaSolution.Data.Data.MappingConfigurations.KPI
{
    public class KPIDetailScoreConfiguration : IEntityTypeConfiguration<KPIDetailScore>
    {
        public void Configure(EntityTypeBuilder<KPIDetailScore> builder)
        {
            builder.ToTable("tbl_KPIDetailScores", schema: "KPI");
            builder.HasKey(e => e.Id);
            builder.HasOne<KPITotalScore>()
             .WithMany()
             .HasForeignKey(u => u.TotalScoreId)
             .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne<KPITask>()
             .WithMany()
             .HasForeignKey(u => u.TaskId)
             .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
