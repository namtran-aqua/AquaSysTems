using AquaSolution.Data.Data.Entities.Admin;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AquaSolution.Data.Data.MappingConfigurations.Admin
{
    public class SystemLockConfiguration : IEntityTypeConfiguration<SystemLock>
    {
        public void Configure(EntityTypeBuilder<SystemLock> builder)
        {
            builder.ToTable("tbl_SystemLock", schema: "Admin");
            builder.HasKey(e => e.Id);
            builder.HasOne<Page>()
              .WithMany()
              .HasForeignKey(u => u.PageId)
              .OnDelete(DeleteBehavior.Restrict);
            builder.HasOne<User>()
              .WithMany()
              .HasForeignKey(u => u.LockedBy)
              .OnDelete(DeleteBehavior.Restrict);

        }
    }
}
