using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using WoopiAiHub.Domain.DTOs.Response.Automation;
using WoopiAiHub.Domain.Enum;
using WoopiAiHub.Domain.Interfaces.Messaging;
using WoopiAiHub.Domain.Interfaces.Services.Automation;
using WoopiAiHub.Infrastructure.Messaging.Configuration;
using WoopiAiHub.Infrastructure.Messaging.Consumers;

namespace WoopiAiHub.Application.Messaging
{
    public class N8NConsumer : BaseConsumer
    {
        private readonly IMessageConsumer<AutomationOutputDto> _consumer;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<N8NConsumer> _logger;
        private readonly MessageQueues _queues;

        public N8NConsumer(IServiceScopeFactory scopeFactory,
                           IConfiguration configuration,
                           IMessageConsumer<AutomationOutputDto> consumer,
                           ILogger<N8NConsumer> logger,
                           IOptions<MessageQueues> queues) : base(configuration)
        {
            _scopeFactory = scopeFactory;
            _queues = queues.Value;
            _consumer = consumer;
            _logger = logger;
        }

        /// <summary>
        /// Execute the n8n consumer's message
        /// </summary>
        /// <param name="stoppingToken"></param>
        /// <returns></returns>
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await _consumer.ConsumerAsync(_queues.AutomationQueueResponse, async message =>
            {
                using var scope = _scopeFactory.CreateScope();
                try
                {
                    var connectionString = await GetConnectionStringAsync(scope, message.Tenant!, ColTypeModule.WoopiAiHub);
                    var httpAccessor = scope.ServiceProvider.GetRequiredService<IHttpContextAccessor>();
                    httpAccessor.HttpContext ??= new DefaultHttpContext();
                    httpAccessor.HttpContext.Items["TenantConnection"] = connectionString;

                    var n8nServices = scope.ServiceProvider.GetRequiredService<IN8NServices>();
                    var automationServicesDto = await n8nServices.ProcessMessage(message);

                    var automationServices = scope.ServiceProvider.GetRequiredService<IAutomationServices>();

                    await automationServices.ContinueExecution(automationServicesDto);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex,
                        "Failed to process message from {QueueName}. Execution id: {ExecutionId}",
                        _queues.AutomationQueueResponse,
                        message.ExecutionId);
                }
            });
        }
    }
}
