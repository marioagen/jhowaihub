using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using WoopiAiHub.Application.Utils;
using WoopiAiHub.Domain.DTOs.Messaging;
using WoopiAiHub.Domain.Enum;
using WoopiAiHub.Domain.Interfaces.Messaging;
using WoopiAiHub.Domain.Interfaces.Services;
using WoopiAiHub.Infrastructure.Messaging.Configuration;
using WoopiAiHub.Infrastructure.Messaging.Consumers;

namespace WoopiAiHub.Application.Messaging.DeadLetter
{
    public class EmbeddingsDeadLetterConsumer(IServiceScopeFactory scopeFactory,
        IConfiguration configuration,
        IMessageConsumer<DocumentEmbeddingsDataDto> consumer,
        ILogger<EmbeddingsDeadLetterConsumer> logger,
        IOptions<MessageQueues> queues) : BaseConsumer(configuration)
    {
        private readonly IMessageConsumer<DocumentEmbeddingsDataDto> _consumer = consumer;
        private readonly IServiceScopeFactory _scopeFactory = scopeFactory;
        private readonly ILogger<EmbeddingsDeadLetterConsumer> _logger = logger;
        private readonly MessageQueues _queues = queues.Value;

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var deadLetterQueue = $"{_queues.EmbeddingQueue}.dlq";

            await _consumer.ConsumerAsync(deadLetterQueue, async message =>
            {
                using var scope = _scopeFactory.CreateScope();
                try
                {
                    if(message.DocumentEmbeddings.Count <= 0)
                    {
                        throw new AppException(ErrorCode.NotFound, "No documents identified", null);
                    }

                    var cardServices = scope.ServiceProvider.GetRequiredService<ICardServices>();
                    await cardServices.SetFailingCard(message.Data.CardId, message.DocumentEmbeddings.First().Email);
                }
                catch (Exception ex)
                {
                    _logger.LogError("Error to process Embeddings DeadLetter: {message}", ex.Message);
                    throw;
                }
            });
        }
    }
}
