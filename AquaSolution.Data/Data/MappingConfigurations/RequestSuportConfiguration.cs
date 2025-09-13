using AquaSolution.Data.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AquaSolution.Data.Data.MappingConfigurations
{
    public class RequestSuportConfiguration : IEntityTypeConfiguration<RequestSuport>
    {
        public void Configure(EntityTypeBuilder<RequestSuport> builder)
        {
            builder.ToTable("tbl_RequestSuports");
            builder.HasKey(e => e.Id);
            builder.HasOne<User>()
                   .WithMany()
                   .HasForeignKey(e => e.RequestBy)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne<User>()
                   .WithMany()
                   .HasForeignKey(e => e.TechnicianId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne<User>()
                   .WithMany()
                   .HasForeignKey(e => e.CreatedById)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne<RequestSuportCategory>()
                   .WithMany()
                   .HasForeignKey(e => e.RequestSuportCategoryId)
                   .OnDelete(DeleteBehavior.Restrict);
            builder.Property(e => e.Status)
            .HasConversion<string>()
            .IsRequired()
            .HasMaxLength(100);
        }
    }
}
