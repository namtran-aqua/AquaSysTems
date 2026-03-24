using AquaSolution.Data.Data.Entities.Admin;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AquaSolution.Data.Data.MappingConfigurations.Admin
{
    public class DepartmentConfiguration : IEntityTypeConfiguration<Department>
    {
        public void Configure(EntityTypeBuilder<Department> builder)
        {
            builder.ToTable("tbl_Departments", schema: "Admin");

            // Primary Key
            builder.HasKey(d => d.Id);

            // Properties
            builder.Property(d => d.Name)
                   .IsRequired()
                   .HasMaxLength(100);

            builder.Property(d => d.Code)
                   .IsRequired()
                   .HasMaxLength(50);

            builder.Property(d => d.Note)
                   .HasMaxLength(2400);
            builder.Property(d => d.DesCription)
            .HasMaxLength(2400);

            builder.Property(d => d.CreatedDate)
                   .IsRequired()
                   .HasDefaultValueSql("GETDATE()");

            builder.Property(e => e.DepartmentType)
                 .HasConversion<string>()
                 .IsRequired()
                 .HasMaxLength(50);
        }
    }
}
