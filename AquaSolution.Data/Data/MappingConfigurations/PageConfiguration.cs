using AquaSolution.Data.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AquaSolution.Data.Data.MappingConfigurations
{
    public class PageConfiguration : IEntityTypeConfiguration<Page>
    {
        public void Configure(EntityTypeBuilder<Page> builder)
        {
            builder.ToTable("tbl_Pages", schema: "Admin");
            builder.HasKey(e => e.Id);
            builder.Property(e => e.Name).IsRequired().HasMaxLength(100);
            builder.Property(e => e.Url).HasMaxLength(500);

            builder.HasOne<Menu>()
                  .WithMany()
                  .HasForeignKey(e => e.MenuId)
                  .OnDelete(DeleteBehavior.Cascade);

        }
    }
}
