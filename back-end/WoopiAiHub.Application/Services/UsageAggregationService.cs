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
        private readonly IUsageDailyRepository _usageDailyRepository;
        private readonly IUsageMonthRepository _usageMonthRepository;
        private readonly ITenantCacheServices _tenantCacheService;
        private readonly IMarketPlaceApi _marketPlaceApi;
        private readonly IConfiguration _configuration;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<UsageAggregationService> _logger;
        private readonly ResiliencePipeline _resiliencePipeline;

        public UsageAggregationService(
            IUsageDailyRepository usageDailyRepository,
            IUsageMonthRepository usageMonthRepository,
            ITenantCacheServices tenantCacheService,
            IMarketPlaceApi marketPlaceApi,
            IConfiguration configuration,
            IServiceScopeFactory scopeFactory,
            ILogger<UsageAggregationService> logger)
        {
            _usageDailyRepository = usageDailyRepository ?? throw new ArgumentNullException(nameof(usageDailyRepository));
            _usageMonthRepository = usageMonthRepository ?? throw new ArgumentNullException(nameof(usageMonthRepository));
            _tenantCacheService = tenantCacheService ?? throw new ArgumentNullException(nameof(tenantCacheService));
            _marketPlaceApi = marketPlaceApi ?? throw new ArgumentNullException(nameof(marketPlaceApi));
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

        public async Task ProcessUnprocessedUsageAsync(CancellationToken ct = default)
        {
            var correlationId = Guid.NewGuid().ToString();
            try
            {
                var keyAccess = _configuration["KeyAccess"] ??
                    throw new InvalidOperationException("KeyAccess not configured");

                var tenants = await _marketPlaceApi.FindAllTenantsByModuleAsync(keyAccess, ColTypeModule.WoopiAiHub);


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

        public async Task ChargeExpiredTenantsAsync(string tenantName, CancellationToken ct = default)
        {
            var correlationId = Guid.NewGuid().ToString();
            var startTime = DateTime.UtcNow;

            _logger.LogInformation("[{CorrelationId}] Starting charge process for tenant: {TenantName}",
                correlationId, tenantName);

            try
            {
                // Get tenant information
                var tenant = await _tenantCacheService.FindTenantAsync(tenantName, ColTypeModule.WoopiAiHub);

                if (tenant == null)
                {
                    _logger.LogWarning("[{CorrelationId}] Tenant not found: {TenantName}", correlationId, tenantName);
                    return;
                }

                // Check if subscription is expired
                if (tenant.DateEnd.HasValue && tenant.DateEnd.Value < DateTime.UtcNow)
                {
                    _logger.LogInformation("[{CorrelationId}] Tenant {TenantName} has expired subscription (DateEnd: {DateEnd})",
                        correlationId, tenantName, tenant.DateEnd.Value);

                    // Calculate period
                    var periodStart = tenant.DateStart ?? DateTime.UtcNow.AddMonths(-1);
                    var periodEnd = tenant.DateEnd.Value;

                    // Get total usage for the tenant
                    var usageByTenant = await _usageMonthRepository.GetTotalUsageByTenantsAsync(periodStart, periodEnd, ct);
                    var totalUsage = usageByTenant.Values.Sum();

                    if (totalUsage > 0)
                    {
                        var chargeRequest = new ChargeRequestDto
                        {
                            TenantName = tenantName,
                            PeriodStart = periodStart,
                            PeriodEnd = periodEnd,
                            TotalUsage = totalUsage
                        };

                        // Call MarketplaceApi with resilience
                        var keyAccess = _configuration["KeyAccess"] ?? throw new InvalidOperationException("KeyAccess not configured");

                        await _resiliencePipeline.ExecuteAsync(async token =>
                        {
                            var result = await _marketPlaceApi.PostChargeAsync(keyAccess, chargeRequest);
                            _logger.LogInformation("[{CorrelationId}] Charge posted successfully for tenant {TenantName}, Total: {TotalUsage}",
                                correlationId, tenantName, totalUsage);
                            return result;
                        }, ct);
                    }
                    else
                    {
                        _logger.LogInformation("[{CorrelationId}] No usage to charge for tenant {TenantName}",
                            correlationId, tenantName);
                    }
                }
                else
                {
                    _logger.LogInformation("[{CorrelationId}] Tenant {TenantName} subscription is still active",
                        correlationId, tenantName);
                }

                var duration = (DateTime.UtcNow - startTime).TotalSeconds;
                _logger.LogInformation("[{CorrelationId}] Charge process completed for tenant {TenantName}, Duration: {Duration}s",
                    correlationId, tenantName, duration);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[{CorrelationId}] Error during charge process for tenant {TenantName}",
                    correlationId, tenantName);
                // Don't rethrow - we don't want one tenant failure to block others
            }
        }
    }
}
