using AquaSolution.Data.Data.Entities.Admin;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AquaSolution.Data.Data.MappingConfigurations.Admin
{

    public class GroupsConfiguration : IEntityTypeConfiguration<Groups>
    {
        public void Configure(EntityTypeBuilder<Groups> builder)
        {
            builder.ToTable("tbl_Groups", schema: "Admin");

            builder.HasKey(g => g.Id);

            builder.Property(g => g.Name)
                   .IsRequired()
                   .HasMaxLength(200);

            builder.Property(g => g.Description)
                   .HasMaxLength(5000);

            builder.Property(g => g.CreatedTime)
                   .IsRequired();

            builder.Property(g => g.CreatedBy)
                   .IsRequired()
                   .HasMaxLength(100);

            builder.Property(g => g.UpdatedTime)
                   .IsRequired(false);

            builder.Property(g => g.UpdateBy)
                   .HasMaxLength(100);
        }
    }
}
