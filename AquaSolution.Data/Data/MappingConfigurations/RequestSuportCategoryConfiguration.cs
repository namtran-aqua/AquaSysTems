using AquaSolution.Data.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AquaSolution.Data.Data.MappingConfigurations
{
    public class RequestSuportCategoryConfiguration : IEntityTypeConfiguration<RequestSuportCategory>
    {
        public void Configure(EntityTypeBuilder<RequestSuportCategory> builder)
        {
            builder.ToTable("tbl_RequestSuportCategorys");
            builder.HasKey(e => e.Id);

            builder.HasOne<User>()
                  .WithMany()
                  .HasForeignKey(e => e.TechnicianId)
                  .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
