using AquaSolution.Data.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AquaSolution.Data.Data.MappingConfigurations
{
    public class InventoryPeriodConfiguration : IEntityTypeConfiguration<InventoryPeriod>
    {
        public void Configure(EntityTypeBuilder<InventoryPeriod> builder)
        {
            builder.ToTable("tbl_InventoryPeriods");
            builder.HasKey(e => e.Id);
            builder.Property(e => e.Name).IsRequired().HasMaxLength(100);
            builder.Property(d => d.CreatedDate)
           .IsRequired()
           .HasDefaultValueSql("GETDATE()");
        }
    }
}
