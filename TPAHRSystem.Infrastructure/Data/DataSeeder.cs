// TPAHRSystem.Infrastructure/Data/DataSeeder.cs
// This file is kept for compatibility but we're using direct SQL scripts instead

namespace TPAHRSystem.Infrastructure.Data
{
    public static class DataSeeder
    {
        public static async Task SeedAsync(TPADbContext context)
        {
            // Data seeding is now handled via SQL scripts
            // See the SQL scripts provided for creating demo users and data
            await Task.CompletedTask;
        }
    }
}