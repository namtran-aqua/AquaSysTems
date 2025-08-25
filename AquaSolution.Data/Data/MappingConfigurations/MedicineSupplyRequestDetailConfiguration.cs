using AquaSolution.Data.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AquaSolution.Data.Data.MappingConfigurations
{
    public class MedicineSupplyRequestDetailConfiguration : IEntityTypeConfiguration<MedicineSupplyRequestDetail>
    {
        public void Configure(EntityTypeBuilder<MedicineSupplyRequestDetail> builder)
        {
            builder.ToTable("tbl_MedicineSupplyRequestDetails");
            builder.HasKey(e => e.Id);
            builder.HasOne<MedicineSupplyRequest>()
               .WithMany()
               .HasForeignKey(u => u.MedicineSupplyRequestId)
               .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne<Product>()
               .WithMany()
               .HasForeignKey(u => u.ProductId)
               .OnDelete(DeleteBehavior.Restrict);

            builder.Property(x => x.QuantityIssued)
              .HasColumnType("decimal(18, 4)")
              .IsRequired(false);
                    builder.Property(x => x.RequestedQuantity)
              .HasColumnType("decimal(18, 4)")
              .IsRequired(false);
        }
    }
}
