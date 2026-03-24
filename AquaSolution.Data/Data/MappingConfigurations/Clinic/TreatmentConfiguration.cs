using AquaSolution.Data.Data.Entities.Clinic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AquaSolution.Data.Data.MappingConfigurations.Clinic
{
    public class TreatmentConfiguration : IEntityTypeConfiguration<Treatment>
    {
        public void Configure(EntityTypeBuilder<Treatment> builder)
        {
            builder.ToTable("tbl_Treatments", schema: "Clinic");
            builder.HasKey(e => e.RequestId);
            builder.Property(d => d.CheckInTime)
                   .IsRequired()
                   .HasDefaultValueSql("GETDATE()");
            builder.Property(d => d.Note)
              .HasMaxLength(4000);
            builder.Property(d => d.Diagnose)
                .HasMaxLength(4000);
            builder.Property(e => e.TreatmentType)
              .HasConversion<string>()
              .IsRequired()
              .HasMaxLength(50);
        }
    }
}
