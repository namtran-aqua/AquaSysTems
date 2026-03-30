using AquaSolution.Data.Data.Entities;
using AquaSolution.Data.Data.Entities.Admin;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AquaSolution.Data.Data.MappingConfigurations.KPI
{
    public class KPIMonthlyActualConfiguration : IEntityTypeConfiguration<KPIMonthlyActual>
    {
        public void Configure(EntityTypeBuilder<KPIMonthlyActual> builder)
        {
            builder.ToTable("tbl_KPIMonthlyActuals", schema: "KPI");
            builder.HasKey(e => e.Id);
            builder.HasOne<User>()
                .WithMany()
                .HasForeignKey(e => e.CreatedBy)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne<KPIMonthlyTarget>()
                  .WithMany()
                  .HasForeignKey(e => e.KPITargetId) 
                  .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne<KPITotalScore>()
                 .WithMany()
                 .HasForeignKey(e => e.KPITotalScoreId)
                 .OnDelete(DeleteBehavior.Restrict);
            builder.Property(d => d.CreatedDate)
               .IsRequired()
               .HasDefaultValueSql("GETDATE()");

        }
    }
}
