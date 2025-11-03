using AquaSolution.Data.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AquaSolution.Data.Data.MappingConfigurations
{
    public class ReportInventoryConfiguration : IEntityTypeConfiguration<ReportInventory>
    {
        public void Configure(EntityTypeBuilder<ReportInventory> builder)
        {
            builder.ToTable("tbl_ReportInventory", schema: "Clinic");
            builder.HasKey(e => e.Id);
            builder.HasOne<User>()
                  .WithMany()
                  .HasForeignKey(e => e.CreatedBy)
                  .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
