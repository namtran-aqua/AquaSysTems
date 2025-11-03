using AquaSolution.Data.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AquaSolution.Data.Data.MappingConfigurations
{
    public class RolePermissionConfiguration : IEntityTypeConfiguration<RolePermission>
    {
        public void Configure(EntityTypeBuilder<RolePermission> builder)
        {
            builder.ToTable("tbl_RolePermissions", schema: "Admin");
            builder.HasKey(e => e.Id);

            builder.HasOne<Role>()
                  .WithMany()
                  .HasForeignKey(e => e.RoleId)
                  .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne<Permission>()
                  .WithMany()
                  .HasForeignKey(e => e.PermissionId)
                  .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
