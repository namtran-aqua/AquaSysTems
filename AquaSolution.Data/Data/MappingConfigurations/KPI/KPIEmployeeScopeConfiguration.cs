using AquaSolution.Data.Data.Entities.KPI;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AquaSolution.Data.Data.MappingConfigurations.KPI
{
    public class KPIEmployeeScopeConfiguration : IEntityTypeConfiguration<KPIEmployeeScope>
    {
        public void Configure(EntityTypeBuilder<KPIEmployeeScope> builder)
        {
            builder.ToTable("tbl_KPIEmployeeScopes", schema: "KPI");
            builder.HasKey(s => s.Id);
            builder.Property(s => s.UserId).IsRequired();
            builder.Property(s => s.Reason).HasMaxLength(500);
            builder.Property(s => s.CreatedDate).IsRequired().HasDefaultValueSql("GETDATE()");

            builder.HasMany(s => s.ExemptMonths)
                   .WithOne(e => e.Scope)
                   .HasForeignKey(e => e.ScopeId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
