
using AquaSolution.Data.Data.Entities.Admin;
using AquaSolution.Data.Data.Entities.Scraps;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AquaSolution.Data.Data.MappingConfigurations.Scraps
{
    public class HistoryScrapConfiguration : IEntityTypeConfiguration<HistoryScrap>
    {
        public void Configure(EntityTypeBuilder<HistoryScrap> builder)
        {
            builder.ToTable("tbl_HistoryScraps", schema: "ScrapManagement");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Status)
                 .HasConversion<string>();

            builder.Property(x => x.ConfirmationStatusType)
                 .HasConversion<string>();

            builder.HasOne<User>()
                 .WithMany()
                 .HasForeignKey(u => u.CreatedBy)
                 .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne<User>()
                 .WithMany()
                 .HasForeignKey(u => u.LastActionBy)
                 .OnDelete(DeleteBehavior.Restrict);
            builder.HasOne<User>()
                 .WithMany()
                 .HasForeignKey(u => u.Confirmer)
                 .OnDelete(DeleteBehavior.Restrict);

            builder.Property(x => x.Notes)
             .HasMaxLength(3000);

            builder.HasOne<Factory>()
                 .WithMany()
                 .HasForeignKey(u => u.FactoryId)
                 .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne<Department>()
              .WithMany()
              .HasForeignKey(u => u.DepartmentId)
              .OnDelete(DeleteBehavior.Restrict);
            builder.Property(e => e.ConfirmAmount)
               .HasColumnType("decimal(18,4)")
               .HasDefaultValue(0m);
            builder.Property(e => e.TotalAmount)
               .HasColumnType("decimal(18,4)")
               .HasDefaultValue(0m);


        }
    }
}
