using AquaSolution.Data.Data.Entities;
using AquaSolution.Data.Data.Entities.KPI;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AquaSolution.Data.Data.MappingConfigurations.KPI
{
    public class KPIActualMasterConfiguration : IEntityTypeConfiguration<KPIActualMaster>
    {
        public void Configure(EntityTypeBuilder<KPIActualMaster> builder)
        {
            builder.ToTable("tbl_KPIActualMasters", schema: "KPI");
            builder.HasKey(e => e.Id);
            builder.HasOne<User>()
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Restrict);
            builder.Property(d => d.CreatedDate)
               .IsRequired()
               .HasDefaultValueSql("GETDATE()");

        }
    }
}
