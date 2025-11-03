using AquaSolution.Data.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AquaSolution.Data.Data.MappingConfigurations
{
    public class WarehouseExportConfiguration : IEntityTypeConfiguration<WarehouseExport>
    {
        public void Configure(EntityTypeBuilder<WarehouseExport> builder)
        {
            builder.ToTable("tbl_WarehouseExports", schema: "Clinic");
            builder.HasKey(e => e.Id);
            builder.Property(e => e.Name).IsRequired().HasMaxLength(500);
            builder.Property(e => e.WarehouseExportType)
                .HasConversion<string>()
                .IsRequired()
                .HasMaxLength(50);
        }
    
    }
}
