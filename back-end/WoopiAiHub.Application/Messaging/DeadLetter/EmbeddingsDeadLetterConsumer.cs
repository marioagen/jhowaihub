using Microsoft.AspNetCore.Http;
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

        /// <summary>
        /// Executes the background processing logic for handling messages in the dead-letter queue associated with
        /// document embeddings.
        /// </summary>
        /// <remarks>This method processes messages from the dead-letter queue and updates failing card
        /// information when document embeddings are present. If the operation is cancelled via the provided token,
        /// processing will stop gracefully.</remarks>
        /// <param name="stoppingToken">A cancellation token that can be used to request the operation to stop processing messages.</param>
        /// <returns>A task that represents the asynchronous execution of the background operation.</returns>
        /// <exception cref="AppException">Thrown if no documents are identified in the received message.</exception>
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

                    var connectionString = await GetConnectionStringAsync(scope, message.DocumentEmbeddings.First().Tenant!);
                    var httpAccessor = scope.ServiceProvider.GetRequiredService<IHttpContextAccessor>();
                    httpAccessor.HttpContext ??= new DefaultHttpContext();
                    httpAccessor.HttpContext.Items["TenantConnection"] = connectionString;

                    var cardServices = scope.ServiceProvider.GetRequiredService<ICardServices>();
                    await cardServices.SetFailingCard(message.Data.CardId, message.DocumentEmbeddings.First().Email);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error to process Embeddings DeadLetter: {Message}", ex.Message);
                    throw;
                }
            });
        }
    }
}
