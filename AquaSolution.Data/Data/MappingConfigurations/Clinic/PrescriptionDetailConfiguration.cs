using AquaSolution.Data.Data.Entities.Clinic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AquaSolution.Data.Data.MappingConfigurations.Clinic
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
