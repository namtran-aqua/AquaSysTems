using AquaSolution.Data.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AquaSolution.Data.Data.MappingConfigurations
{
    //public class ButtonConfiguration : IEntityTypeConfiguration<Button>
    //{
    //    public void Configure(EntityTypeBuilder<Button> builder)
    //    {
    //        builder.ToTable("Buttons");
    //        builder.HasKey(e => e.Id);
    //        builder.Property(e => e.Name).IsRequired().HasMaxLength(100);

    //        builder.HasOne<Page>()
    //              .WithMany()
    //              .HasForeignKey(e => e.PageId)
    //              .OnDelete(DeleteBehavior.Cascade);
    //    }
    //}
}
