using AquaSolution.Data.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AquaSolution.Data.Data.MappingConfigurations
{
    public class WarehouseImportDetailConfiguration : IEntityTypeConfiguration<WarehouseImportDetail>
    {
        public void Configure(EntityTypeBuilder<WarehouseImportDetail> builder)
        {
            builder.ToTable("tbl_WarehouseImportDetails", schema: "Clinic");
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
            builder.HasOne<WarehouseImport>()
                 .WithMany()
                 .HasForeignKey(e => e.WarehouseImportId)
                 .OnDelete(DeleteBehavior.Cascade);

        }
    }
}
