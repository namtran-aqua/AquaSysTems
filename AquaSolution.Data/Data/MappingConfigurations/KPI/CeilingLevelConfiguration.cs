using AquaSolution.Data.Data.Entities.Admin;
using AquaSolution.Data.KPI.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AquaSolution.Data.Data.MappingConfigurations.KPI
{
    public class CeilingLevelConfiguration : IEntityTypeConfiguration<CeilingLevel>
    {
        public void Configure(EntityTypeBuilder<CeilingLevel> builder)
        {
            builder.ToTable("tbl_CeilingLevels", schema: "KPI");
            builder.HasOne<Factory>()
             .WithMany()
             .HasForeignKey(u => u.FactoryId)
             .OnDelete(DeleteBehavior.Restrict);
            builder.HasOne<User>()
             .WithMany()
             .HasForeignKey(u => u.CreatedBy)
             .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
