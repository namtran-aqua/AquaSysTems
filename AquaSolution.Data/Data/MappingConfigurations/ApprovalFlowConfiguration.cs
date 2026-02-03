using AquaSolution.Data.Data.Entities;
using AquaSolution.Shared.Enum;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AquaSolution.Data.Data.MappingConfigurations
{
    public class ApprovalFlowConfiguration : IEntityTypeConfiguration<ApprovalFlow>
    {
        public void Configure(EntityTypeBuilder<ApprovalFlow> builder)
        {
            builder.ToTable("tbl_ApprovalFlows", schema: "Admin");
            builder.HasKey(e => e.Id);
            builder.Property(e => e.Name).IsRequired().HasMaxLength(100);

            builder.Property(e => e.ApprovalSettingType)
                    .HasConversion<string>()
                    .IsRequired();
            builder.Property(e => e.SystemType)
                .HasDefaultValue(SystemType.KPI)
               .HasConversion<string>()
               .IsRequired();
            builder.Property(e => e.FlowApproval)
                   .IsRequired()
                   .HasDefaultValue(0);

        }
    }
}
