using AquaSolution.Data.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AquaSolution.Data.Data.MappingConfigurations
{
    public class ProductConfiguration : IEntityTypeConfiguration<Product>
    {
        public void Configure(EntityTypeBuilder<Product> builder)
        {
            builder.ToTable("tbl_Products", schema: "Clinic");
            builder.HasKey(e => e.Id);
            builder.Property(e => e.Name).IsRequired().HasMaxLength(100);
            builder.Property(e => e.ProductType)
                    .HasConversion<string>()
                    .IsRequired()
                    .HasMaxLength(100);
            builder.Property(d => d.Note)
                   .HasMaxLength(2400);
            builder.Property(d => d.CreatedDate)
             .IsRequired()
             .HasDefaultValueSql("GETDATE()");
        }
    }
}
