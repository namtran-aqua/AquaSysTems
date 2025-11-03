using AquaSolution.Data.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AquaSolution.Data.Data.MappingConfigurations
{
    public class InventoryConfiguration : IEntityTypeConfiguration<Inventories>
    {
        public void Configure(EntityTypeBuilder<Inventories> builder)
        {
            builder.ToTable("tbl_Inventory", schema: "Clinic");
            builder.HasKey(e => e.Id);
            builder.Property(x => x.Quantity)
           .HasColumnType("decimal(18, 4)")
           .IsRequired();
            builder.HasOne<Product>()
            .WithMany()
            .HasForeignKey(u => u.ProductId)
            .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
