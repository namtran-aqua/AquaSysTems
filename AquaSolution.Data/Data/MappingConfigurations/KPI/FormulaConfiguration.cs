using AquaSolution.Data.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AquaSolution.Data.Data.MappingConfigurations
{
    public class FormulaConfiguration : IEntityTypeConfiguration<Formula>
    {
        public void Configure(EntityTypeBuilder<Formula> builder)
        {
            builder.ToTable("tbl_Formulas");
            builder.HasKey(e => e.Id);
            builder.Property(e => e.FormulaName).IsRequired().HasMaxLength(100);
            builder.Property(e => e.KPIFormulaType)
                    .HasConversion<string>()
                    .IsRequired()
                    .HasMaxLength(100);
        }
    }
}
