using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using WoopiAiHub.Domain.DTOs.Refit;
using WoopiAiHub.Domain.Enum;
using WoopiAiHub.Domain.Interfaces.Repository;
using WoopiAiHub.Domain.Interfaces.Repository.Cache;
using WoopiAiHub.Domain.Interfaces.Services;
using WoopiAiHub.Domain.Models;

namespace WoopiAiHub.Application.Services
{
    public class UsageAggregationService : IUsageAggregationService
    {
        private readonly ITenantCacheServices _tenantCacheService;
        private readonly IConfiguration _configuration;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<UsageAggregationService> _logger;

        public UsageAggregationService(ITenantCacheServices tenantCacheService,
                                       IConfiguration configuration,
                                       IServiceScopeFactory scopeFactory,
                                       ILogger<UsageAggregationService> logger)
        {
            _tenantCacheService = tenantCacheService ?? throw new ArgumentNullException(nameof(tenantCacheService));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _scopeFactory = scopeFactory ?? throw new ArgumentNullException(nameof(scopeFactory));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Processes unprocessed usage data for all tenants associated with the specified module.
        /// </summary>
        /// <remarks>This method retrieves all tenants associated with the module and processes their
        /// metrics.  If no tenants are found, the method logs the information and exits. For each tenant, it 
        /// processes tenant metrics by aggregating daily usage into monthly summaries.</remarks>
        /// <param name="ct">A <see cref="CancellationToken"/> that can be used to cancel the operation.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        /// <exception cref="InvalidOperationException">Thrown if the required configuration value for "KeyAccess" is not set.</exception>
        public async Task ProcessUnprocessedUsageAsync()
        {
            var keyAccess = _configuration["KeyAccess"] ??
                throw new InvalidOperationException("KeyAccess not configured");

            var tenants = await _tenantCacheService.FindAllTenantsAsync(ColTypeModule.WoopiAiHub);

            if (!tenants.Any())
            {
                return;
            }

            foreach (var tenant in tenants)
            {
                await ProcessTenantMetricsAsync(tenant);
            }
        }

        /// <summary>
        /// Process metrics for a specific tenant using its database connection
        /// </summary>
        private async Task ProcessTenantMetricsAsync(TenantListDto tenant)
        {
            using var scope = _scopeFactory.CreateScope();
            var connectionString = FormatConnectionStringAsync(tenant);
            var httpAccessor = scope.ServiceProvider.GetRequiredService<IHttpContextAccessor>();
            httpAccessor.HttpContext ??= new DefaultHttpContext();
            httpAccessor.HttpContext.Items["TenantConnection"] = connectionString;

            var usageDailyRepository = scope.ServiceProvider.GetRequiredService<IUsageDailyRepository>();
            var usageMonthRepository = scope.ServiceProvider.GetRequiredService<IUsageMonthRepository>();

            var unprocessedRecords = await usageDailyRepository.FindUnprocessedAsync();

            if (!unprocessedRecords.Any())
            {
                _logger.LogDebug("No unprocessed records found for tenant {TenantName}", tenant.Name);
                return;
            }

            var grouped = unprocessedRecords
                .GroupBy(ud => new
                {
                    ud.UsageTypeId,
                    ud.ModelEmbeddingId,
                    ud.UserId,
                    Day = ud.Created.Date
                })
                .ToList();

            foreach (var group in grouped)
            {
                var totalUsage = group.Sum(ud => ud.UsageCount);
                var dailyRecord = new UsageMonth(
                    id: 0,
                    created: group.Key.Day,
                    usageTypeId: group.Key.UsageTypeId,
                    total: totalUsage,
                    modelEmbeddingId: group.Key.ModelEmbeddingId,
                    userId: group.Key.UserId
                );

                await usageMonthRepository.UpsertAsync(dailyRecord);
            }
            var recordIds = unprocessedRecords.Select(ud => ud.Id).ToList();
            await usageDailyRepository.MarkAsProcessedAsync(recordIds);
        }

        /// <summary>
        /// Get connection string for a specific tenant
        /// </summary>
        private string FormatConnectionStringAsync(TenantListDto tenant)
        {
            var template = _configuration.GetConnectionString("TemplateConnection");
            var connectionString = template?.Replace("___NEWDB___", tenant!.DatabaseName);

            return connectionString!;
        }
    }
}
