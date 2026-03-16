using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using WoopiAiHub.Domain.DTOs.Request.Automation;
using WoopiAiHub.Domain.Interfaces.Messaging;
using WoopiAiHub.Domain.Interfaces.Services;
using WoopiAiHub.Infrastructure.Messaging.Configuration;
using WoopiAiHub.Infrastructure.Messaging.Consumers;

namespace WoopiAiHub.Application.Messaging.DeadLetter
{
    public class N8NDeadLetterConsumer(IServiceScopeFactory scopeFactory,
        IConfiguration configuration,
        IMessageConsumer<AutomationInputDto> consumer,
        ILogger<N8NDeadLetterConsumer> logger,
        IOptions<MessageQueues> queues) : BaseConsumer(configuration)
    {
        private readonly IMessageConsumer<AutomationInputDto> _consumer = consumer;
        private readonly IServiceScopeFactory _scopeFactory = scopeFactory;
        private readonly ILogger<N8NDeadLetterConsumer> _logger = logger;
        private readonly MessageQueues _queues = queues.Value;

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var deadLetterQueue = $"{_queues.AutomationQueueConsumer}.dlq";

            await _consumer.ConsumerAsync(deadLetterQueue, async message =>
            {
                using var scope = _scopeFactory.CreateScope();
                try
                {
                    var cardServices = scope.ServiceProvider.GetRequiredService<ICardServices>();
                    await cardServices.SetFailingCard(message.Data.CardId, message.Email);
                }
                catch (Exception ex)
                {
                    _logger.LogError("Error to process N8N DeadLetter: {message}", ex.Message);
                    throw;
                }
            });
        }
    }
}
