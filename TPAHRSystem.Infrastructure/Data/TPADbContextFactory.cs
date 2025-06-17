// TPAHRSystem.Infrastructure/Data/TPADbContextFactory.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace TPAHRSystem.Infrastructure.Data
{
    public class TPADbContextFactory : IDesignTimeDbContextFactory<TPADbContext>
    {
        public TPADbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<TPADbContext>();

            // Default connection string for design time
            var connectionString = "Server=(localdb)\\mssqllocaldb;Database=TPAHRSystem;Trusted_Connection=true;MultipleActiveResultSets=true";

            // Try to get connection string from appsettings.json if available
            try
            {
                var apiProjectPath = Path.Combine(Directory.GetCurrentDirectory(), "..", "TPAHRSystem.API");

                var configuration = new ConfigurationBuilder()
                    .AddJsonFile(Path.Combine(apiProjectPath, "appsettings.json"), optional: true)
                    .Build();

                var configConnectionString = configuration.GetConnectionString("DefaultConnection");
                if (!string.IsNullOrEmpty(configConnectionString))
                {
                    connectionString = configConnectionString;
                }
            }
            catch
            {
                // If we can't read the config, use the default connection string
            }

            optionsBuilder.UseSqlServer(connectionString);

            return new TPADbContext(optionsBuilder.Options);
        }
    }
}