using AquaSolution.Data.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AquaSolution.Data.Data.MappingConfigurations
{
    public class PrescriptionDetailConfiguration : IEntityTypeConfiguration<PrescriptionDetail>
    {
        public void Configure(EntityTypeBuilder<PrescriptionDetail> builder)
        {
            builder.ToTable("tbl_PrescriptionDetails", schema: "Clinic");
            builder.HasKey(e => e.Id);
            builder.HasOne<Prescription>()
           .WithMany()
           .HasForeignKey(e => e.PrescriptionId)
           .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
