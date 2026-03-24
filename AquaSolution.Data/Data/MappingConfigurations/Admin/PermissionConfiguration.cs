using AquaSolution.Data.Data.Entities.Admin;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AquaSolution.Data.Data.MappingConfigurations.Admin
{
    public class PermissionConfiguration : IEntityTypeConfiguration<Permission>
    {
        public void Configure(EntityTypeBuilder<Permission> builder)
        {
            builder.ToTable("tbl_Permissions", schema: "Admin");
            builder.HasKey(e => e.Id);

            builder.Property(e => e.Action)
                  .HasConversion<string>()
                  .IsRequired()
                  .HasMaxLength(50);

            builder.Property(e => e.Type)
                  .HasConversion<string>()
                  .IsRequired()
                  .HasMaxLength(50);
        }
    }
}
