using AquaSolution.Data.Data.Entities;
using AquaSolution.Data.KPI.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AquaSolution.Data.Data.MappingConfigurations.KPI
{
    public class UserTaskConfiguration : IEntityTypeConfiguration<UserTask>
    {
        public void Configure(EntityTypeBuilder<UserTask> builder)
        {
            builder.ToTable("tbl_UserTasks", schema: "KPI");
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
