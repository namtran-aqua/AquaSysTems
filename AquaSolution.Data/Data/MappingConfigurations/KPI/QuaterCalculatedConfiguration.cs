using AquaSolution.Data.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AquaSolution.Data.Data.MappingConfigurations
{
    public class QuaterCalculatedConfiguration : IEntityTypeConfiguration<QuaterCalculated>
    {
        public void Configure(EntityTypeBuilder<QuaterCalculated> builder)
        {
            builder.ToTable("tbl_QuaterCalculateds");
            builder.HasKey(e => e.Id);
            builder.Property(e => e.Calculated).IsRequired().HasMaxLength(100);
            builder.Property(e => e.KPIQuarterCalculateType)
                    .HasConversion<string>()
                    .IsRequired()
                    .HasMaxLength(100);
        }
    }
}
