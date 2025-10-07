using AquaSolution.Data.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AquaSolution.Data.Data.MappingConfigurations
{
    public class UserTaskConfiguration : IEntityTypeConfiguration<UserTask>
    {
        public void Configure(EntityTypeBuilder<UserTask> builder)
        {
            builder.ToTable("tbl_UserTasks");
            builder.HasKey(e => e.Id);
            builder.HasOne<User>()
             .WithMany()
             .HasForeignKey(u => u.UserId)
             .OnDelete(DeleteBehavior.Restrict);
            builder.HasOne<KPITask>()
             .WithMany()
             .HasForeignKey(u => u.KPITaskId)
             .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
