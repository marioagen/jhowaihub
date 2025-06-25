using DocAnalyzer.Repository.Context;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DocAnalyzer.Repository
{
    public class TenantRepository : ITenantRepository
    {
        private readonly IServiceProvider serviceProvider;
        private readonly IHostingEnvironment _environment;
        private readonly IConfiguration _config;
        private readonly ILogger<TenantRepository> _logger;

        public TenantRepository(IServiceProvider serviceProvider,
                                IHostingEnvironment environment,
                                IConfiguration config,
                                ILogger<TenantRepository> logger)
        {
            this.serviceProvider = serviceProvider;
            this._environment = environment;
            this._config = config;
            this._logger = logger;
        }

        /// <summary>
        /// Performs the initial configuration of the tenant 
        /// creating a database if it does not exist
        /// </summary>
        /// <returns></returns>
        public bool CreateDatabase()
        {
            try
            {
                using (var serviceScope = serviceProvider?.GetService<IServiceScopeFactory>()?.CreateScope())
                {
                    var context = serviceScope?.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                    if (context != null &&
                        context.Database != null &&
                        context.Database.CanConnect() is false)
                    {
                        context.Database.SetCommandTimeout(600);
                        context.Database.Migrate();

                        if (_environment.IsDevelopment() is true)
                            return true;

                        var newPlan = _config["DatabaseSettings:DefaultPlan"];
                        context.Database.ExecuteSqlRaw($"ALTER DATABASE [{context?.Database.GetDbConnection().Database}] " +
                                                       $"MODIFY (EDITION = '{newPlan}')");

                        return true;
                    }
                    else if (context != null &&
                             context.Database != null &&
                             context.Database.CanConnect() is true)
                    {
                        return true;
                    }

                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while creating the database. Operation: CreateDatabase");
                this.RemoveDataBaseCatch();
                throw;
            }
        }

        /// <summary>
        /// Removes the created database if caught in the method
        /// </summary>
        private void RemoveDataBaseCatch()
        {
            using (var serviceScope = serviceProvider?.GetService<IServiceScopeFactory>()?.CreateScope())
            {
                var context = serviceScope?.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                if (context != null && context.Database.CanConnect())
                {
                    context.Database.EnsureDeleted();
                }
            }
        }
    }
}

