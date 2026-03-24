using AquaSolution.Data.Data.Entities.Clinic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AquaSolution.Data.Data.MappingConfigurations.Clinic
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
            builder.Property(x => x.IsActive)
                .HasDefaultValue(true)
                .IsRequired();
        }
    }
}
