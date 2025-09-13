using AquaSolution.Data.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AquaSolution.Data.Data.MappingConfigurations
{
    public class WarehouseImportConfiguration : IEntityTypeConfiguration<WarehouseImport>
    {
        public void Configure(EntityTypeBuilder<WarehouseImport> builder)
        {
            builder.ToTable("tbl_WarehouseImports");
            builder.HasKey(e => e.Id);
            builder.Property(e => e.Name).IsRequired().HasMaxLength(500);
            builder.Property(e => e.WarehouseImportType)
                .HasConversion<string>()
                .IsRequired()
                .HasMaxLength(50);
        }
    
    }
}
