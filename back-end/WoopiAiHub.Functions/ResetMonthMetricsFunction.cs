using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using WoopiAiHub.Domain.Interfaces.Services;

namespace WoopiAiHub.Functions
{
    public class ResetMonthMetricsFunction
    {
        private readonly IUsageArchiveService _usageArchiveService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<ResetMonthMetricsFunction> _logger;

        public ResetMonthMetricsFunction(
            IUsageArchiveService usageArchiveService,
            IConfiguration configuration,
            ILogger<ResetMonthMetricsFunction> logger)
        {
            _usageArchiveService = usageArchiveService ?? throw new ArgumentNullException(nameof(usageArchiveService));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [Function("ResetMonthMetrics")]
        public async Task Run([TimerTrigger("%UsageManagement:ResetMetricsCron%")] TimerInfo myTimer)
        {
            var correlationId = Guid.NewGuid().ToString();
            var startTime = DateTime.UtcNow;

            _logger.LogInformation("[{CorrelationId}] ResetMonthMetrics function started at: {StartTime}",
                correlationId, startTime);

            try
            {
                await _usageArchiveService.ArchiveOldUsageAsync();

                var duration = (DateTime.UtcNow - startTime).TotalSeconds;
                _logger.LogInformation("[{CorrelationId}] ResetMonthMetrics function completed. Duration: {Duration}s",
                    correlationId, duration);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[{CorrelationId}] Error in ResetMonthMetrics function", correlationId);
                throw;
            }
        }
    }
}
