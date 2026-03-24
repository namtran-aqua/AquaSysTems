using AquaSolution.Data.Data.Entities.Clinic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AquaSolution.Data.Data.MappingConfigurations.Clinic
{
    public class MedicineConfiguration : IEntityTypeConfiguration<Medicine>
    {
        public void Configure(EntityTypeBuilder<Medicine> builder)
        {
            builder.ToTable("tbl_Medicines", schema: "Clinic");
            builder.HasKey(e => e.ProducId);

            builder.Property(e => e.ProductType)
             .HasConversion<string>()
             .IsRequired()
             .HasMaxLength(100);
            builder.HasOne<Product>()
                   .WithMany()
                   .HasForeignKey(u => u.ProducId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
