using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace DocAnalyzer.Repository.Util
{
    /// <summary>
    /// Running the migration and creating the database, if it has not already been created.
    /// </summary>
    public static class InitApplicationDb
    {
        public static void RunApplicationMigration(DbContext context)
        {
            Console.WriteLine("--> Attempting to apply migrations...");
            try
            {
                if (context != null &&
                    context.Database != null &&
                    context.Database.CanConnect() is true &&
                    context.Database.GetPendingMigrations().Any())
                {
                    context.Database.Migrate();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"--> Could not run migrations: {ex.Message}");
            }
        }
    }
}