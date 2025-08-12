using AquaSolution.Data.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AquaSolution.Data.Data.MappingConfigurations
{
    public class InventoryPeriodDetailConfiguration : IEntityTypeConfiguration<InventoryPeriodDetail>
    {
        public void Configure(EntityTypeBuilder<InventoryPeriodDetail> builder)
        {
            builder.ToTable("tbl_InventoryPeriodDetails");
            builder.HasKey(e => new { e.InventoryId, e.InventoryPeriodId });

            builder.Property(e => e.ProductType)
                .HasConversion<string>()
                .IsRequired()
                .HasMaxLength(50);

            builder.HasOne<InventoryPeriod>()
                .WithMany()
                .HasForeignKey(e => e.InventoryPeriodId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne<Product>()
                .WithMany()
                .HasForeignKey(e => e.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

        }
    }
}
