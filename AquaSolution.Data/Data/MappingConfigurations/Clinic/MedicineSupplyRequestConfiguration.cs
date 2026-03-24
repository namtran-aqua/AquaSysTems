using AquaSolution.Data.Data.Entities.Admin;
using AquaSolution.Data.Data.Entities.Clinic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AquaSolution.Data.Data.MappingConfigurations.Clinic
{
    public class MedicineSupplyRequestConfiguration : IEntityTypeConfiguration<MedicineSupplyRequest>
    {
        public void Configure(EntityTypeBuilder<MedicineSupplyRequest> builder)
        {
            builder.ToTable("tbl_MedicineSupplyRequests", schema: "Clinic");
            builder.HasKey(e => e.Id);
            builder.HasOne<Department>()
               .WithMany()
               .HasForeignKey(u => u.DepartmentId)
               .OnDelete(DeleteBehavior.Restrict);
            builder.HasOne<Factory>()
               .WithMany()
               .HasForeignKey(u => u.FactoryId)
               .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne<User>()
               .WithMany()
               .HasForeignKey(u => u.UserRequestId)
               .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne<User>()
               .WithMany()
               .HasForeignKey(u => u.CreatedById)
               .OnDelete(DeleteBehavior.Restrict);
            builder.HasOne<User>()
               .WithMany()
               .HasForeignKey(u => u.ApprovalId)
               .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne<User>()
               .WithMany()
               .HasForeignKey(u => u.PharmacyManagerId)
               .OnDelete(DeleteBehavior.Restrict);

            builder.Property(e => e.RequestType)
             .HasConversion<string>()
             .IsRequired()
             .HasMaxLength(50);

        }
    }
}
