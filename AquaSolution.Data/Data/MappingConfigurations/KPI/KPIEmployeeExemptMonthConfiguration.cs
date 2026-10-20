using AquaSolution.Data.Data.Entities.KPI;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AquaSolution.Data.Data.MappingConfigurations.KPI
{
    public class KPIEmployeeExemptMonthConfiguration : IEntityTypeConfiguration<KPIEmployeeExemptMonth>
    {
        public void Configure(EntityTypeBuilder<KPIEmployeeExemptMonth> builder)
        {
            builder.ToTable("tbl_KPIEmployeeExemptMonths", schema: "KPI");
            builder.HasKey(e => e.Id);
            builder.Property(e => e.UserId).IsRequired();
            builder.Property(e => e.Year).IsRequired();
            builder.Property(e => e.Month).IsRequired();
            builder.Property(e => e.Note).HasMaxLength(250);
            builder.Property(e => e.CreatedDate).IsRequired().HasDefaultValueSql("GETDATE()");

            // Ràng buộc Unique ngăn chặn 1 nhân viên bị trùng 2 bản ghi miễn cho cùng 1 tháng/năm
            builder.HasIndex(e => new { e.UserId, e.Year, e.Month })
                   .IsUnique();

            builder.HasIndex(e => e.ScopeId);
        }
    }
}
