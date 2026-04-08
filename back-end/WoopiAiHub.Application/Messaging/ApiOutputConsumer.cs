using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using WoopiAiHub.Domain.DTOs;
using WoopiAiHub.Domain.DTOs.Response;
using WoopiAiHub.Domain.Interfaces.Messaging;
using WoopiAiHub.Domain.Interfaces.Services;
using WoopiAiHub.Domain.Interfaces.Services.Automation;
using WoopiAiHub.Domain.Utils;
using WoopiAiHub.Infrastructure.Messaging.Configuration;
using WoopiAiHub.Infrastructure.Messaging.Consumers;

namespace WoopiAiHub.Application.Messaging
{
    public class ApiOutputConsumer(IServiceScopeFactory scopeFactory,
        IConfiguration configuration,
        IMessageConsumer<ApiOutputDto> consumer,
        ILogger<ApiOutputConsumer> logger,
        IOptions<MessageQueues> queues) : BaseConsumer(configuration)
    {
        private readonly IMessageConsumer<ApiOutputDto> _consumer = consumer;
        private readonly IServiceScopeFactory _scopeFactory = scopeFactory;
        private readonly ILogger<ApiOutputConsumer> _logger = logger;
        private readonly MessageQueues _queues = queues.Value;

        /// <summary>
        /// Executes the background service and processes messages from the API request queue until the operation is
        /// cancelled.
        /// </summary>
        /// <remarks>The method listens for messages on the API request queue and processes each message
        /// in a scoped service context. Processing stops when the provided cancellation token is triggered.</remarks>
        /// <param name="stoppingToken">A cancellation token that can be used to signal the request to stop processing messages.</param>
        /// <returns>A task that represents the asynchronous execution of the background service.</returns>
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await _consumer.ConsumerAsync(_queues.ApiRequestQueueResponse, async message =>
            {
                using var scope = _scopeFactory.CreateScope();
                AutomationServicesDto? automationServicesDto = null;
                try
                {
                    var connectionString = await GetConnectionStringAsync(scope, message.Tenant!);
                    var httpAccessor = scope.ServiceProvider.GetRequiredService<IHttpContextAccessor>();
                    httpAccessor.HttpContext ??= new DefaultHttpContext();
                    httpAccessor.HttpContext.Items["TenantConnection"] = connectionString;

                    var apiServices = scope.ServiceProvider.GetRequiredService<IApiOutputServices>();
                    automationServicesDto = await apiServices.ProcessMessage(message);

                    var automationServices = scope.ServiceProvider.GetRequiredService<IAutomationServices>();
                    var usageDailyServices = scope.ServiceProvider.GetRequiredService<IUsageDailyServices>();

                    await usageDailyServices.AddByValuesAsync(MetricNames.Automation, message.Email!, 1);

                    await automationServices.ContinueExecution(automationServicesDto);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex,
                        "Failed to process message from {QueueName}. Execution id: {ExecutionId}",
                        _queues.ApiRequestQueueResponse,
                        message.ExecutionId);

                    if (automationServicesDto?.CardId > 0)
                    {
                        try
                        {
                            var failingCardService = scope.ServiceProvider.GetRequiredService<IFailingCardService>();
                            await failingCardService.SetFailingCard(automationServicesDto.CardId, message.Email);
                        }
                        catch (Exception failingEx)
                        {
                            _logger.LogError(failingEx, "Erro ao marcar card {CardId} como failing após exception do consumer", automationServicesDto.CardId);
                        }
                    }
                }
            });
        }
    }
}
