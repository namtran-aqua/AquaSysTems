using AquaSolution.Data.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AquaSolution.Data.Data.MappingConfigurations
{
    public class WarehouseExportDetailConfiguration : IEntityTypeConfiguration<WarehouseExportDetail>
    {
        public void Configure(EntityTypeBuilder<WarehouseExportDetail> builder)
        {
            builder.ToTable("tbl_WarehouseExportDetails", schema: "Clinic");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Quantity)
                .HasColumnType("decimal(18, 4)") 
                .IsRequired();

            builder.Property(x => x.DateManufacture)
                .HasColumnType("datetime2");

            builder.Property(x => x.ExpiryDate)
                .HasColumnType("datetime2");

            builder.Property(x => x.ProductType)
                .HasConversion<string>();
            builder.HasOne<Product>()
                 .WithMany()
                 .HasForeignKey(e => e.ProductId)
                 .OnDelete(DeleteBehavior.Cascade);
            builder.HasOne<WarehouseExport>()
                 .WithMany()
                 .HasForeignKey(e => e.WarehouseExportId)
                 .OnDelete(DeleteBehavior.Cascade);

        }
    }
}
