using AquaSolution.Data.Data.Entities;
using Azure.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AquaSolution.Data.Data.MappingConfigurations
{
    public class PrescriptionConfiguration : IEntityTypeConfiguration<Prescription>
    {
        public void Configure(EntityTypeBuilder<Prescription> builder)
        {
            builder.ToTable("tbl_Prescriptions");
            builder.HasKey(e => e.Id);
            builder.Property(d => d.Note)
         .HasMaxLength(4000);
            builder.HasOne<RequestClinic>()
         .WithMany()
         .HasForeignKey(e => e.RequestId)
         .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
