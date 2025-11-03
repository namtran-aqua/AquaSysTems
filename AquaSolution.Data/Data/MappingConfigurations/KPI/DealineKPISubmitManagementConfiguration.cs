using AquaSolution.Data.Data.Entities;
using AquaSolution.Data.Data.Entities.KPI;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AquaSolution.Data.Data.MappingConfigurations.KPI
{
    public class DealineKPISubmitManagementConfiguration : IEntityTypeConfiguration<DealineKPISubmitManagement>
    {
        public void Configure(EntityTypeBuilder<DealineKPISubmitManagement> builder)
        {
            builder.ToTable("tbl_DealineKPISubmitManagements",schema:"KPI");
            builder.Property(d => d.CreatedDate)
                 .IsRequired()
                 .HasDefaultValueSql("GETDATE()");
            builder.Property(d => d.StartDate)
                 .IsRequired()
                 .HasDefaultValueSql("GETDATE()");
                        builder.Property(d => d.EndDate)
                 .IsRequired()
                 .HasDefaultValueSql("GETDATE()");
        }
    }
}
