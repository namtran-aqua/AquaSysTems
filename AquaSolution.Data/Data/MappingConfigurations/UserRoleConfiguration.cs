using AquaSolution.Data.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AquaSolution.Data.Data.MappingConfigurations
{
    public class UserRoleConfiguration : IEntityTypeConfiguration<UserRole>
    {
        public void Configure(EntityTypeBuilder<UserRole> builder)
        {
            builder.ToTable("tbl_UserRoles", schema: "Admin");
            builder.HasKey(e => new { e.UserId, e.RoleId });

            builder.HasOne<User>()
                  .WithMany()
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne<Role>()
                  .WithMany()
                  .HasForeignKey(e => e.RoleId)
                  .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
