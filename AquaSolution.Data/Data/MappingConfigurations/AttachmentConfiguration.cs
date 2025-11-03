using AquaSolution.Data.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AquaSolution.Data.Data.MappingConfigurations
{
    public class AttachmentConfiguration : IEntityTypeConfiguration<Attachment>
    {
        public void Configure(EntityTypeBuilder<Attachment> builder)
        {
            builder.ToTable("tbl_Attachments", schema: "RequestSuport");
            builder.HasKey(e => e.Id);

            builder.HasOne<RequestSuport>()
                   .WithMany()
                   .HasForeignKey(e => e.RequestSuportId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
