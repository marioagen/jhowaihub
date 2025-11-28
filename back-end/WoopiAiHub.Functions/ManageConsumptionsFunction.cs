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
