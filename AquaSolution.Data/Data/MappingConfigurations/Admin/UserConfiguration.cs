using AquaSolution.Data.Data.Entities.Admin;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AquaSolution.Data.Data.MappingConfigurations.Admin
{
    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.ToTable("tbl_Users", schema: "Admin");

            builder.HasKey(e => e.Id);

            builder.Property(e => e.FirstName)
                   .IsRequired()
                   .HasMaxLength(100);
            builder.Property(e => e.LastName)
                   .IsRequired()
                   .HasMaxLength(100);

            builder.Property(e => e.WorkDayId)
                   .IsRequired()
                   .HasMaxLength(100);

            builder.Property(e => e.Email)
                   .HasMaxLength(1000);

            builder.HasOne<Department>() 
                   .WithMany()
                   .HasForeignKey(u => u.DepartmentId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne<Factory>() 
                   .WithMany()
                   .HasForeignKey(u => u.FactoryId)
                   .OnDelete(DeleteBehavior.Restrict);
            builder.HasOne<Position>() 
                   .WithMany()
                   .HasForeignKey(u => u.PositionId)
                   .OnDelete(DeleteBehavior.Restrict);
            builder.Property(e => e.FlowApproval)
                     .HasDefaultValue(0);
        }
    }
}
