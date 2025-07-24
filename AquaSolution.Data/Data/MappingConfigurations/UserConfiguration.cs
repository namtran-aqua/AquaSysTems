using AquaSolution.Data.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AquaSolution.Data.Data.MappingConfigurations
{
    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.ToTable("tbl_Users");

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
            builder.HasOne<Groups>() 
                   .WithMany()
                   .HasForeignKey(u => u.GroupId)
                   .OnDelete(DeleteBehavior.Restrict);
                    }
    }
}
