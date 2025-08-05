using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using WoopiAiHub.Domain.Enum;
using WoopiAiHub.Domain.Interfaces.Repository.Cache;
using WoopiAiHub.Repository.Context;
using WoopiAiHub.Repository.Util;

namespace WoopiAiHub.Infrastructure.Multitenancy
{
    public class TenantContextService : ITenantContextService
    {
        private readonly IConfiguration _configuration;
        private readonly ITenantCacheServices _tenantCacheService;

        public TenantContextService(IConfiguration configuration,
                                    ITenantCacheServices tenantCacheService)
        {
            _configuration = configuration;
            _tenantCacheService = tenantCacheService;
        }

        /// <summary>
        /// Initializes the tenant-specific database and applies necessary migrations.
        /// </summary>
        /// <remarks>This method retrieves the tenant information using the provided identifier,
        /// constructs a connection string for the tenant's database, and applies any pending database migrations. It
        /// ensures that the tenant's database is properly initialized and ready for use.</remarks>
        /// <param name="tenantIdentifier">The unique identifier of the tenant. This value cannot be null, empty, or consist only of whitespace.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException">Thrown if <paramref name="tenantIdentifier"/> is null, empty, or consists only of whitespace.</exception>
        /// <exception cref="InvalidOperationException">Thrown if the tenant specified by <paramref name="tenantIdentifier"/> cannot be found.</exception>
        public async Task InitializeTenantAsync(string tenantIdentifier)
        {
            if (string.IsNullOrWhiteSpace(tenantIdentifier))
                throw new ArgumentException("Tenant identifier cannot be null or empty.", nameof(tenantIdentifier));

            var tenant = await _tenantCacheService.FindTenantAsync(tenantIdentifier,
                                                                   ColTypeModule.WoopiAiHub);
            if (tenant == null)
                throw new InvalidOperationException($"Tenant '{tenantIdentifier}' not found.");

            var tenantDbName = tenant.DatabaseName;

            var connectionString = BuildConnectionString(tenantDbName);

            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlServer(connectionString)
                .Options;

            using var ctx = new ApplicationDbContext(options);
            await Task.Run(() => InitApplicationDb.RunApplicationMigration(ctx));
        }

        /// <summary>
        /// Attempts to set the tenant-specific database connection string in the provided HTTP context.
        /// </summary>
        /// <remarks>This method retrieves tenant information based on the provided <paramref
        /// name="tenantIdentifier"/> and constructs a database connection string for the tenant. The connection string
        /// is stored in the <see cref="HttpContext.Items"/> collection under the key "TenantConnection". If the tenant
        /// cannot be found or the identifier is invalid, the method returns <see langword="false"/>.</remarks>
        /// <param name="context">The <see cref="HttpContext"/> in which the tenant connection string will be stored.</param>
        /// <param name="tenantIdentifier">The unique identifier of the tenant. Cannot be null, empty, or whitespace.</param>
        /// <returns><see langword="true"/> if the tenant connection string was successfully set; otherwise, <see
        /// langword="false"/>.</returns>
        public async Task<bool> TrySetTenantConnectionAsync(HttpContext context, string tenantIdentifier)
        {
            if (string.IsNullOrWhiteSpace(tenantIdentifier)) return false;

            var tenant = await _tenantCacheService.FindTenantAsync(tenantIdentifier, ColTypeModule.WoopiAiHub);
            if (tenant == null) return false;

            var connectionString = BuildConnectionString(tenant.DatabaseName);
            context.Items["TenantConnection"] = connectionString;
            return true;
        }

        /// <summary>
        /// Builds a connection string for the specified tenant database.
        /// </summary>
        /// <param name="tenantDbName">The name of the tenant database to include in the connection string. Cannot be null or empty.</param>
        /// <returns>A connection string with the tenant database name substituted into the template.</returns>
        /// <exception cref="InvalidOperationException">Thrown if the "TemplateConnection" connection string is not configured in the application settings.</exception>
        private string BuildConnectionString(string tenantDbName)
        {
            var template = _configuration.GetConnectionString("TemplateConnection")
                           ?? throw new InvalidOperationException("TemplateConnection not configured");
            return template.Replace("___NEWDB___", tenantDbName);
        }
    }
}
