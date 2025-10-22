using AquaSolution.Data.Data.Entities;
using AquaSolution.Data.Data.Entities.KPI;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AquaSolution.Data.Data.MappingConfigurations.KPI
{
    public class KPIIndexWeightConfiguration : IEntityTypeConfiguration<KPIIndexWeight>
    {
        public void Configure(EntityTypeBuilder<KPIIndexWeight> builder)
        {
            builder.ToTable("tbl_KPIIndexWeights");
            builder.HasKey(e => e.Id);
            builder.Property(e => e.PeriodType)
                    .HasConversion<string>()
                    .IsRequired()
                    .HasMaxLength(100);
            builder.Property(e => e.PositionType)
                  .HasConversion<string>()
                  .IsRequired()
                  .HasMaxLength(100);
            builder.Property(e => e.KPIIndexType)
                  .HasConversion<string>()
                  .IsRequired()
                  .HasMaxLength(100);
        }
    }
}
