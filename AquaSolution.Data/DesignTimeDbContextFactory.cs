
using AquaSolution.Data.Connection;
using Microsoft.EntityFrameworkCore.Design;

namespace AquaData
{
    public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<AquaDbContext>
    {
        public AquaDbContext CreateDbContext(string[] args)
        {
            // Gọi lại DbContextConfigurator của bạn
            var options = DbContextConfigurator.GetDbContextOptions();
            return new AquaDbContext(options);
        }
    }
}
