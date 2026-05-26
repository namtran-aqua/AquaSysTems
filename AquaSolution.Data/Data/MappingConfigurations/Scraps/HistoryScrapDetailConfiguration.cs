
using AquaSolution.Data.Data.Entities.Scraps;
using AquaSolution.Data.Entities.Scraps;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AquaSolution.Data.Data.MappingConfigurations.Scraps
{
    public class HistoryScrapDetailConfiguration : IEntityTypeConfiguration<HistoryScrapDetail>
    {
        public void Configure(EntityTypeBuilder<HistoryScrapDetail> builder)
        {
            builder.ToTable("tbl_HistoryScrapDetails", schema: "ScrapManagement");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Plant)
                 .HasConversion<string>();

            builder.HasOne<HistoryScrap>()
                 .WithMany()
                 .HasForeignKey(u => u.ScrapHistoryId)
                 .OnDelete(DeleteBehavior.Restrict);
            builder.HasOne<Material>()
                 .WithMany()
                 .HasForeignKey(u => u.MaterialId)
                 .OnDelete(DeleteBehavior.Restrict);

            builder.Property(e => e.Quantity)
                 .HasColumnType("decimal(18,4)")
                 .HasDefaultValue(0m);


            builder.Property(e => e.Weight)
                 .HasColumnType("decimal(18,4)")
                 .HasDefaultValue(0m);

            builder.Property(e => e.TotalWeight)
                 .HasColumnType("decimal(18,4)")
                 .HasDefaultValue(0m);
            builder.Property(x => x.Reson)
       .HasMaxLength(3000);


        }
    }
}
