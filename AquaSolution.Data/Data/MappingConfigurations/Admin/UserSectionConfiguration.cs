using AquaSolution.Data.Data.Entities.Admin;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AquaSolution.Data.Data.MappingConfigurations.Admin
{
    public class UserSectionConfiguration : IEntityTypeConfiguration<UserSection>
    {
        public void Configure(EntityTypeBuilder<UserSection> builder)
        {
            builder.ToTable("tbl_UserSections", "Admin");

            builder.HasKey(us => new { us.UserId, us.SectionId });

            builder.HasOne(us => us.User)
                .WithMany(u => u.UserSections)
                .HasForeignKey(us => us.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(us => us.Section)
                .WithMany()
                .HasForeignKey(us => us.SectionId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
