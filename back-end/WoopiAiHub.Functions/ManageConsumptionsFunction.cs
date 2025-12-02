using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using WoopiAiHub.Domain.Interfaces.Services;

namespace WoopiAiHub.Functions
{
    public class ManageConsumptionsFunction
    {
        private readonly IUsageAggregationService _usageAggregationService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<ManageConsumptionsFunction> _logger;

        public ManageConsumptionsFunction(
            IUsageAggregationService usageAggregationService,
            IConfiguration configuration,
            ILogger<ManageConsumptionsFunction> logger)
        {
            _usageAggregationService = usageAggregationService ?? throw new ArgumentNullException(nameof(usageAggregationService));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Executes the ManageConsumptions function, triggered by a timer, to process unprocessed usage data.
        /// </summary>
        /// <remarks>This function is triggered based on the cron expression specified in the application
        /// configuration  under the key "UsageManagement:ManageConsumptionsCron". It processes unprocessed usage data
        /// by  invoking the usage aggregation service. Any exceptions encountered during execution are logged  and
        /// rethrown.</remarks>
        /// <param name="myTimer">The timer trigger information, including the schedule and invocation details.</param>
        /// <returns></returns>
        [Function("ManageConsumptions")]
        public async Task Run([TimerTrigger("%UsageManagement:ManageConsumptionsCron%")] TimerInfo myTimer)
        {
            try
            {
                await _usageAggregationService.ProcessUnprocessedUsageAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in ManageConsumptions function");
                throw;
            }
        }
    }
}
