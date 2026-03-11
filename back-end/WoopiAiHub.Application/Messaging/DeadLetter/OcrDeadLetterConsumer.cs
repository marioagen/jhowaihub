using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using WoopiAiHub.Domain.DTOs.Messaging;
using WoopiAiHub.Domain.Interfaces.Messaging;
using WoopiAiHub.Domain.Interfaces.Services;
using WoopiAiHub.Infrastructure.Messaging.Configuration;
using WoopiAiHub.Infrastructure.Messaging.Consumers;

namespace WoopiAiHub.Application.Messaging.DeadLetter
{
    public class OcrDeadLetterConsumer(IServiceScopeFactory scopeFactory,
        IConfiguration configuration,
        IMessageConsumer<ProcessOcrDto> consumer,
        ILogger<OcrDeadLetterConsumer> logger,
        IOptions<MessageQueues> queues) : BaseConsumer(configuration)
    {
        private readonly IMessageConsumer<ProcessOcrDto> _consumer = consumer;
        private readonly IServiceScopeFactory _scopeFactory = scopeFactory;
        private readonly ILogger<OcrDeadLetterConsumer> _logger = logger;
        private readonly MessageQueues _queues = queues.Value;

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var deadLetterQueue = $"{_queues.OcrQueue}.dlq";

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
                    _logger.LogError($"Error to process OCR DeadLetter: {ex.Message}");
                    throw;
                }
            });
        }
    }
}
