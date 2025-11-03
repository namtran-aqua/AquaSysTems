using AquaSolution.Data.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AquaSolution.Data.Data.MappingConfigurations
{
    public class RequestClinicConfiguration : IEntityTypeConfiguration<RequestClinic>
    {
        public void Configure(EntityTypeBuilder<RequestClinic> builder)
        {
            builder.ToTable("tbl_RequestClinics", schema: "Clinic");
            builder.HasKey(e => e.Id);

            builder.HasOne<User>()
                   .WithMany()
                   .HasForeignKey(u => u.UserRequestId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne<User>()
                   .WithMany()
                   .HasForeignKey(u => u.ApprovalBy)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne<User>()
                   .WithMany()
                   .HasForeignKey(u => u.PharmacyManagerId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne<User>()
                   .WithMany()
                   .HasForeignKey(u => u.CreatedBy)
                   .OnDelete(DeleteBehavior.Restrict);


            builder.Property(e => e.Status)
             .HasConversion<string>()
             .IsRequired();
            builder.Property(e => e.PurposeType)
             .HasConversion<string>()
             .IsRequired();


            builder.Property(d => d.Note)
             .HasMaxLength(2400);

            builder.Property(d => d.HistoryReuqest)
             .HasMaxLength(4000);

            builder.Property(d => d.CreatedDate)
                   .IsRequired()
                   .HasDefaultValueSql("GETDATE()");
        }

    }
}
