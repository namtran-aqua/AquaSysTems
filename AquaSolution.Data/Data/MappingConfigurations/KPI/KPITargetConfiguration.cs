using AquaSolution.Data.Data.Entities;
using AquaSolution.Data.Data.Entities.Admin;
using AquaSolution.Data.KPI.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AquaSolution.Data.Data.MappingConfigurations.KPI
{
    public class KPIMonthlyTargetConfiguration : IEntityTypeConfiguration<KPIMonthlyTarget>
    {
        public void Configure(EntityTypeBuilder<KPIMonthlyTarget> builder)
        {
            builder.ToTable("tbl_KPIMonthlyTargets", schema: "KPI");
            builder.HasKey(e => e.Id);
            builder.HasOne<UserTask>()
                .WithMany()
                .HasForeignKey(e => e.UserTaskId)
                .OnDelete(DeleteBehavior.Restrict);
            builder.HasOne<User>()
                .WithMany()
                .HasForeignKey(e => e.UpdatedBy)
                .OnDelete(DeleteBehavior.Restrict);
            builder.HasOne<User>()
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Restrict);
            builder.Property(d => d.CreatedDate)
               .IsRequired()
               .HasDefaultValueSql("GETDATE()");
            builder.Property(x => x.TargetValue)
               .HasColumnType("decimal(18,4)")
               .IsRequired();
        }
    }
}
