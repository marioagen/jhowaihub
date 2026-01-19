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
using WoopiAiHub.Domain.Interfaces.Services;
using WoopiAiHub.Domain.Models;

namespace WoopiAiHub.Application.Services
{
    public class UsageArchiveService : IUsageArchiveService
    {
        private readonly IMarketPlaceApi _marketPlaceApi;
        private readonly IConfiguration _configuration;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<UsageArchiveService> _logger;
        private readonly ResiliencePipeline _resiliencePipeline;

        public UsageArchiveService(IMarketPlaceApi marketPlaceApi,
                                   IConfiguration configuration,
                                   IServiceScopeFactory scopeFactory,
                                   ILogger<UsageArchiveService> logger)
        {
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

        public async Task ArchiveOldUsageAsync()
        {
            var keyAccess = _configuration["KeyAccess"] ??
                throw new InvalidOperationException("KeyAccess not configured");

            var tenants = await _marketPlaceApi.FindAllTenantsByModuleAsync(keyAccess, ColTypeModule.WoopiAiHub);

            if (!tenants.Any())
            {
                _logger.LogInformation("No tenants found");
                return;
            }

            foreach (var tenant in tenants)
            {
                await ArchiveTenantOldUsageAsync(tenant);
                await SendMonthlyUsageIfExpiredAsync(tenant, keyAccess);
            }
        }

        /// <summary>
        /// Archive old usage records for a specific tenant
        /// </summary>
        private async Task ArchiveTenantOldUsageAsync(TenantListDto tenant)
        {
            using var scope = _scopeFactory.CreateScope();
            var connectionString = FormatConnectionStringAsync(tenant);
            var httpAccessor = scope.ServiceProvider.GetRequiredService<IHttpContextAccessor>();
            httpAccessor.HttpContext ??= new DefaultHttpContext();
            httpAccessor.HttpContext.Items["TenantConnection"] = connectionString;
            var usageDailyRepository = scope.ServiceProvider.GetRequiredService<IUsageDailyRepository>();
            var usageLogRepository = scope.ServiceProvider.GetRequiredService<IUsageLogRepository>();

            var monthsThreshold = _configuration.GetValue<int>("UsageManagement:ArchiveMonthsThreshold", 3);
            var cutoffDate = DateTime.UtcNow.AddMonths(-monthsThreshold);

            var oldRecords = await usageDailyRepository.FindOldRecordsAsync(cutoffDate);

            if (!oldRecords.Any())
            {
                return;
            }

            var logsToInsert = new List<UsageLog>();

            foreach (var record in oldRecords)
            {
                var exists = await usageLogRepository.ExistsAsync(record.Id, record.Created);

                if (!exists)
                {
                    var logEntry = new UsageLog(
                        0,
                        created: record.Created,
                        userId: record.UserId,
                        usageTypeId: record.UsageTypeId,
                        usageCount: record.UsageCount,
                        processed: record.Processed,
                        modelEmbeddingId: record.ModelEmbeddingId
                    );

                    logsToInsert.Add(logEntry);
                }
            }

            if (logsToInsert.Any())
            {
                await usageLogRepository.BulkInsertAsync(logsToInsert);
            }

            var recordIds = oldRecords.Select(r => r.Id).ToList();
            await usageDailyRepository.BulkDeleteAsync(recordIds);
        }

        /// <summary>
        /// Send monthly usage to Microsoft if tenant subscription has expired
        /// </summary>
        private async Task SendMonthlyUsageIfExpiredAsync(TenantListDto tenant,
                                                          string keyAccess)
        {
            using var scope = _scopeFactory.CreateScope();
            var connectionString = FormatConnectionStringAsync(tenant);
            var httpAccessor = scope.ServiceProvider.GetRequiredService<IHttpContextAccessor>();
            httpAccessor.HttpContext ??= new DefaultHttpContext();
            httpAccessor.HttpContext.Items["TenantConnection"] = connectionString;
            var usageMonthRepository = scope.ServiceProvider.GetRequiredService<IUsageMonthRepository>();
            var subcriptionPeriodService = scope.ServiceProvider.GetRequiredService<ISubscriptionPeriodServices>();

            var lastPeriod = await subcriptionPeriodService.FindLastUnprocessedAsync();
            if (lastPeriod == null)
            {
                _logger.LogDebug("No unprocessed subscription period found for tenant {TenantName}", tenant.Name);
                return;
            }

            var usageByTenant = await usageMonthRepository.FindTotalUsageAsync(lastPeriod.PeriodStart, lastPeriod.PeriodEnd);

            if (usageByTenant <= 0)
            {
                _logger.LogInformation("No usage to charge for tenant {TenantName}", tenant.Name);
                return;
            }

            var chargeRequest = new ExcessManagementTenantDto
            {
                Tenant = tenant.Name,
                UsageCount = usageByTenant,
            };

            await _resiliencePipeline.ExecuteAsync(async token =>
            {
                var result = await _marketPlaceApi.ProcessConsumption(keyAccess, chargeRequest);
                if (result)
                {
                    _logger.LogInformation("Successfully sent usage charge for tenant {TenantName}", tenant.Name);
                    await subcriptionPeriodService.UpdateToProcessedAsync(lastPeriod.Id);
                }
                else
                {
                    _logger.LogError("Failed to send usage charge for tenant {TenantName}.", tenant.Name);
                }
                return result;
            });
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
