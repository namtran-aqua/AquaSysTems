using AquaSolution.Data.Data.Entities.Clinic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AquaSolution.Data.Data.MappingConfigurations.Clinic
{
    public class MedicalSupliesConfiguration : IEntityTypeConfiguration<MedicalSuplies>
    {
        public void Configure(EntityTypeBuilder<MedicalSuplies> builder)
        {
            builder.HasKey(e => e.ProducId);
            builder.ToTable("tbl_MedicalSuply",schema:"Clinic");
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
