using AquaSolution.Data.Data.Entities.Admin;
using AquaSolution.Data.Data.Entities.RequestITSuports;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AquaSolution.Data.Data.MappingConfigurations.RequestITSuport
{
    public class RequestSuportCategoryConfiguration : IEntityTypeConfiguration<RequestSuportCategory>
    {
        public void Configure(EntityTypeBuilder<RequestSuportCategory> builder)
        {
            builder.ToTable("tbl_RequestSuportCategorys", schema: "RequestSuport");
            builder.HasKey(e => e.Id);

            builder.HasOne<User>()
                  .WithMany()
                  .HasForeignKey(e => e.TechnicianId)
                  .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
