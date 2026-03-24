using AquaSolution.Data.Data.Entities.Clinic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AquaSolution.Data.Data.MappingConfigurations.Clinic
{
    public class ReportInventoryDetailConfiguration : IEntityTypeConfiguration<ReportInventoryDetail>
    {
        public void Configure(EntityTypeBuilder<ReportInventoryDetail> builder)
        {
            builder.ToTable("tbl_ReportInventoryDetail", schema: "Clinic");
            builder.HasKey(e => e.Id);


            builder.HasOne<ReportInventory>()
                  .WithMany()
                  .HasForeignKey(e => e.ReportInventoryId)
                  .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne<Product>()
                  .WithMany()
                  .HasForeignKey(e => e.ProductId)
                  .OnDelete(DeleteBehavior.Cascade);

        }
    }
}
