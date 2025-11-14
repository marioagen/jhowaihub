using Google.Api;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using WoopiAiHub.Domain.Models;
using WoopiAiHub.Repository.Context;

namespace WoopiAiHub.Repository
{
    public class TenantRepository : ITenantRepository
    {
        private readonly IServiceProvider serviceProvider;
        private readonly IHostingEnvironment _environment;
        private readonly IConfiguration _config;
        private readonly ILogger<TenantRepository> _logger;
        private readonly ApplicationDbContext _context;

        public TenantRepository(IServiceProvider serviceProvider,
                                IHostingEnvironment environment,
                                IConfiguration config,
                                ILogger<TenantRepository> logger,
                                ApplicationDbContext context)
        {
            this.serviceProvider = serviceProvider;
            this._environment = environment;
            this._config = config;
            this._logger = logger;
            _context = context;
        }

        /// <summary>
        /// Creates a unique new tenant in the database.
        /// </summary>
        /// <remarks>This method attempts to add the specified tenant to the system. Ensure that the
        /// tenant object contains all required information before calling this method. The operation may fail if the
        /// tenant already exists or if there are validation errors.</remarks>
        /// <param name="tenant">The tenant to be created. Must not be <see langword="null"/>.</param>
        /// <returns><see langword="true"/> if the tenant was successfully created; otherwise, <see langword="false"/>.</returns>
        public bool CreateUniqueTenant(Tenant tenant)
        {
            var exists = _context.Tenants.Any(t => t.Name == tenant.Name);
            if (!exists)
            {
                _context.Tenants.Add(tenant);
                _context.SaveChanges();
                return true;
            }
            return false;
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

