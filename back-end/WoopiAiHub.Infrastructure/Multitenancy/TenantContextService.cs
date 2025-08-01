using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Collections.Concurrent;
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
        private static readonly ConcurrentDictionary<string, SemaphoreSlim> _initLocks = new();

        public TenantContextService(IConfiguration configuration,
                                    ITenantCacheServices tenantCacheService)
        {
            _configuration = configuration;
            _tenantCacheService = tenantCacheService;
        }

        public async Task InitializeTenantAsync(string tenantIdentifier)
        {
            if (string.IsNullOrWhiteSpace(tenantIdentifier))
                throw new ArgumentException("Tenant identifier cannot be null or empty.", nameof(tenantIdentifier));

            // resolve tenant metadata (incluindo DatabaseName)
            var tenant = await _tenantCacheService.FindTenantAsync(tenantIdentifier, ColTypeModule.WoopiAiHub);
            if (tenant == null)
                throw new InvalidOperationException($"Tenant '{tenantIdentifier}' not found.");

            var tenantDbName = tenant.DatabaseName;

            var sem = _initLocks.GetOrAdd(tenantIdentifier, _ => new SemaphoreSlim(1, 1));
            await sem.WaitAsync();
            try
            {
                var connectionString = BuildConnectionString(tenantDbName);

                var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                    .UseSqlServer(connectionString)
                    .Options;

                using var ctx = new ApplicationDbContext(options);
                await Task.Run(() => InitApplicationDb.RunApplicationMigration(ctx));
            }
            finally
            {
                sem.Release();
            }
        }

        public async Task<bool> TrySetTenantConnectionAsync(HttpContext context, string tenantIdentifier)
        {
            if (string.IsNullOrWhiteSpace(tenantIdentifier)) return false;

            var tenant = await _tenantCacheService.FindTenantAsync(tenantIdentifier, ColTypeModule.WoopiAiHub);
            if (tenant == null) return false;

            var connectionString = BuildConnectionString(tenant.DatabaseName);
            context.Items["TenantConnection"] = connectionString;
            return true;
        }

        private string BuildConnectionString(string tenantDbName)
        {
            var template = _configuration.GetConnectionString("TemplateConnection")
                           ?? throw new InvalidOperationException("TemplateConnection not configured");
            return template.Replace("___NEWDB___", tenantDbName);
        }
    }
}
