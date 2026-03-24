using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using WoopiAiHub.Domain.DTOs;
using WoopiAiHub.Domain.DTOs.Messaging;
using WoopiAiHub.Domain.Interfaces.Messaging;
using WoopiAiHub.Domain.Interfaces.Services;
using WoopiAiHub.Infrastructure.Messaging.Configuration;
using WoopiAiHub.Infrastructure.Messaging.Consumers;

namespace WoopiAiHub.Application.Messaging.DeadLetter
{
    public class QuizDeadLetterConsumer(IServiceScopeFactory scopeFactory,
        IConfiguration configuration,
        IMessageConsumer<DocumentEmbeddingsQueryDto> consumer,
        ILogger<QuizDeadLetterConsumer> logger,
        IOptions<MessageQueues> queues) : BaseConsumer(configuration)
    {
        private readonly IMessageConsumer<DocumentEmbeddingsQueryDto> _consumer = consumer;
        private readonly IServiceScopeFactory _scopeFactory = scopeFactory;
        private readonly ILogger<QuizDeadLetterConsumer> _logger = logger;
        private readonly MessageQueues _queues = queues.Value;

        /// <summary>
        /// Executes the background processing logic for handling messages in the dead-letter queue asynchronously.
        /// </summary>
        /// <remarks>This method is called by the host to start processing messages from the dead-letter
        /// queue. Processing continues until the cancellation token is triggered. Exceptions encountered during message
        /// handling are logged and rethrown.</remarks>
        /// <param name="stoppingToken">A cancellation token that can be used to request cancellation of the background operation.</param>
        /// <returns>A task that represents the asynchronous execution of the background operation.</returns>
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var deadLetterQueue = $"{_queues.AnswerQueue}.dlq";

            await _consumer.ConsumerAsync(deadLetterQueue, async message =>
            {
                using var scope = _scopeFactory.CreateScope();
                try
                {
                    var connectionString = await GetConnectionStringAsync(scope, message.Tenant!);
                    var httpAccessor = scope.ServiceProvider.GetRequiredService<IHttpContextAccessor>();
                    httpAccessor.HttpContext ??= new DefaultHttpContext();
                    httpAccessor.HttpContext.Items["TenantConnection"] = connectionString;

                    var data = message.Data.ToObject<MetaDataAutomationDto>();

                    var cardServices = scope.ServiceProvider.GetRequiredService<ICardServices>();
                    await cardServices.SetFailingCard(data.CardId, message.Email);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error to process Quiz DeadLetter: {Message}", ex.Message);
                    throw;
                }
            });
        }
    }
}
