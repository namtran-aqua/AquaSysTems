using AquaSolution.Data.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AquaSolution.Data.Data.MappingConfigurations
{
    public class PermissionConfiguration : IEntityTypeConfiguration<Permission>
    {
        public void Configure(EntityTypeBuilder<Permission> builder)
        {
            builder.ToTable("tbl_Permissions");
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
