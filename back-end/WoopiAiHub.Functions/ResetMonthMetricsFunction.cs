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

        /// <summary>
        /// Executes the ResetMonthMetrics function, triggered by a timer, to archive old usage metrics.
        /// </summary>
        /// <remarks>This function is triggered based on the cron expression specified in the
        /// <c>UsageManagement:ResetMetricsCron</c> configuration. It generates a unique correlation ID for logging
        /// purposes and archives old usage metrics using the <c>_usageArchiveService</c>. Logs are recorded for the
        /// start time, completion time, and duration of the operation. Any exceptions encountered during execution are
        /// logged and rethrown.</remarks>
        /// <param name="myTimer">The timer trigger information, including the schedule defined by the <c>UsageManagement:ResetMetricsCron</c>
        /// configuration setting.</param>
        /// <returns></returns>
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
