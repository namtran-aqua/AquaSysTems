using AquaSolution.Data.Data.Entities;
using AquaSolution.Data.Data.Entities.Admin;
using AquaSolution.Data.Data.Entities.KPI;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AquaSolution.Data.Data.MappingConfigurations.KPI
{
    public class KPITaskConfiguration : IEntityTypeConfiguration<KPITask>
    {
        public void Configure(EntityTypeBuilder<KPITask> builder)
        {
            builder.ToTable("tbl_KPITasks", schema: "KPI");
            builder.HasKey(e => e.Id);
            builder.Property(e => e.TaskName).IsRequired().HasMaxLength(100);
            builder.Property(e => e.KPICategory)
                    .HasConversion<string>()
                    .IsRequired()
                    .HasMaxLength(100);
            builder.Property(e => e.KPIIndexType)
                     .HasConversion<string>()
                     .IsRequired()
                     .HasMaxLength(100);
            builder.HasOne<Formula>()
                     .WithMany()
                     .HasForeignKey(u => u.FormulaId)
                     .OnDelete(DeleteBehavior.Restrict);
            builder.HasOne<QuarterCalculate>()
                     .WithMany()
                     .HasForeignKey(u => u.CalculatedId)
                     .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne<Factory>()
                     .WithMany()
                     .HasForeignKey(u => u.FactoryId)
                     .OnDelete(DeleteBehavior.Restrict);
            builder.HasOne<Department>()
                     .WithMany()
                     .HasForeignKey(u => u.DepartmentId)
                     .OnDelete(DeleteBehavior.Restrict);
            builder.HasOne<User>()
                     .WithMany()
                     .HasForeignKey(u => u.CreatedById)
                     .OnDelete(DeleteBehavior.Restrict);
            builder.Property(d => d.CreatedDate)
                       .IsRequired()
                       .HasDefaultValueSql("GETDATE()");
        }
    }
}
