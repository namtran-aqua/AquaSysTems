using AquaSolution.Data.Data.Entities.Clinic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AquaSolution.Data.Data.MappingConfigurations.Clinic
{
    public class InventoryPeriodConfiguration : IEntityTypeConfiguration<InventoryPeriod>
    {
        public void Configure(EntityTypeBuilder<InventoryPeriod> builder)
        {
            builder.ToTable("tbl_InventoryPeriods", schema: "Clinic");
            builder.HasKey(e => e.Id);
            builder.Property(e => e.Name).IsRequired().HasMaxLength(100);
            builder.Property(d => d.CreatedDate)
           .IsRequired()
           .HasDefaultValueSql("GETDATE()");
        }
    }
}
