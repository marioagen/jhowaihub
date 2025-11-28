using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.CircuitBreaker;
using Polly.Retry;
using WoopiAiHub.Domain.DTOs.Refit;
using WoopiAiHub.Domain.Enum;
using WoopiAiHub.Domain.Interfaces.Refit;
using WoopiAiHub.Domain.Interfaces.Repository;
using WoopiAiHub.Domain.Interfaces.Repository.Cache;
using WoopiAiHub.Domain.Interfaces.Services;
using WoopiAiHub.Domain.Models;

namespace WoopiAiHub.Application.Services
{
    public class UsageAggregationService : IUsageAggregationService
    {
        private readonly IMarketPlaceApi _marketPlaceApi;
        private readonly ITenantCacheServices _tenantCacheService;
        private readonly IConfiguration _configuration;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<UsageAggregationService> _logger;
        private readonly ResiliencePipeline _resiliencePipeline;

        public UsageAggregationService(IMarketPlaceApi marketPlaceApi,
                                       ITenantCacheServices tenantCacheService,
                                       IConfiguration configuration,
                                       IServiceScopeFactory scopeFactory,
                                       ILogger<UsageAggregationService> logger)
        {
            _marketPlaceApi = marketPlaceApi ?? throw new ArgumentNullException(nameof(marketPlaceApi));
            _tenantCacheService = tenantCacheService ?? throw new ArgumentNullException(nameof(tenantCacheService));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _scopeFactory = scopeFactory ?? throw new ArgumentNullException(nameof(scopeFactory));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            // Configure Polly resilience pipeline
            _resiliencePipeline = new ResiliencePipelineBuilder()
                .AddRetry(new RetryStrategyOptions
                {
                    MaxRetryAttempts = 3,
                    Delay = TimeSpan.FromSeconds(2),
                    BackoffType = DelayBackoffType.Exponential,
                    OnRetry = args =>
                    {
                        _logger.LogWarning("Retry attempt {AttemptNumber} after {Delay}ms due to: {Exception}",
                            args.AttemptNumber, args.RetryDelay.TotalMilliseconds, args.Outcome.Exception?.Message);
                        return ValueTask.CompletedTask;
                    }
                })
                .AddCircuitBreaker(new CircuitBreakerStrategyOptions
                {
                    FailureRatio = 0.5,
                    MinimumThroughput = 5,
                    SamplingDuration = TimeSpan.FromSeconds(30),
                    BreakDuration = TimeSpan.FromSeconds(30),
                    OnOpened = args =>
                    {
                        _logger.LogError("Circuit breaker opened due to: {Exception}", args.Outcome.Exception?.Message);
                        return ValueTask.CompletedTask;
                    }
                })
                .Build();
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
        public async Task ProcessUnprocessedUsageAsync(CancellationToken ct = default)
        {
            var correlationId = Guid.NewGuid().ToString();
            try
            {
                var keyAccess = _configuration["KeyAccess"] ??
                    throw new InvalidOperationException("KeyAccess not configured");

                var tenants = await _tenantCacheService.FindAllTenantsAsync(ColTypeModule.WoopiAiHub);

                if (!tenants.Any())
                {
                    _logger.LogInformation("[{CorrelationId}] No tenants found", correlationId);
                    return;
                }

                foreach (var tenant in tenants)
                {
                    await ProcessTenantMetricsAsync(tenant);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in ProcessUnprocessedUsageAsync");
                throw;
            }
        }



        /// <summary>
        /// Process metrics for a specific tenant using its database connection
        /// </summary>
        private async Task ProcessTenantMetricsAsync(TenantListDto tenant)
        {
            using var scope = _scopeFactory.CreateScope();
            var connectionString = FormatConnectionStringAsync(scope, tenant);
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
        private string FormatConnectionStringAsync(IServiceScope scope,
                                                   TenantListDto tenant)
        {
            var template = _configuration.GetConnectionString("TemplateConnection");
            var connectionString = template?.Replace("___NEWDB___", tenant!.DatabaseName);

            return connectionString!;
        }
    }
}
