using AquaSolution.Data.Data.Entities.Imgs;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AquaSolution.Data.Data.MappingConfigurations.Imgs
{
    public class FolderPermissionConfiguration : IEntityTypeConfiguration<FolderPermission>
    {
        public void Configure(EntityTypeBuilder<FolderPermission> builder)
        {
            builder.ToTable("tbl_FolderPermissions", schema: "Imgs");

            builder.HasKey(d => d.Id);

            builder.Property(d => d.WorkDayId)
                   .IsRequired();

            builder.Property(d => d.FolderId)
                   .IsRequired()
                   .HasMaxLength(250);

            builder.Property(d => d.FolderName)
                   .IsRequired()
                   .HasMaxLength(250);
        }
    }
}
