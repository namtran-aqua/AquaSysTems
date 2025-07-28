using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
namespace AquaSolution.Data.Connection
{
    public class DbContextConfigurator
    {
        public static DbContextOptions<AquaDbContext> GetDbContextOptions()
        {
         var configuration = new ConfigurationBuilder()
        .SetBasePath(AppContext.BaseDirectory)
        .AddJsonFile("appsettings.data.json", optional: false, reloadOnChange: true)
        .Build();
            var connectionString = configuration.GetConnectionString("DefaultConnection");

            if (string.IsNullOrEmpty(connectionString))
            {
                throw new InvalidOperationException("Connection string 'DefaultConnection' not found in appsettings.json.");
            }

            var optionsBuilder = new DbContextOptionsBuilder<AquaDbContext>();
            optionsBuilder.UseSqlServer(connectionString);

            return optionsBuilder.Options;
        }
    }
}
