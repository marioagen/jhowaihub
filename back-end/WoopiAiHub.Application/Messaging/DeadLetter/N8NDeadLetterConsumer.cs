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

        /// <summary>
        /// Executes the background processing logic for handling messages from the dead-letter queue asynchronously.
        /// </summary>
        /// <remarks>This method processes messages from the dead-letter queue and marks associated cards
        /// as failing. The operation runs continuously until the provided cancellation token is triggered.</remarks>
        /// <param name="stoppingToken">A cancellation token that can be used to request cancellation of the background operation.</param>
        /// <returns>A task that represents the asynchronous execution of the background operation.</returns>
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
                    _logger.LogError(ex, "Error to process N8N DeadLetter: {Message}", ex.Message);
                    throw;
                }
            });
        }
    }
}
