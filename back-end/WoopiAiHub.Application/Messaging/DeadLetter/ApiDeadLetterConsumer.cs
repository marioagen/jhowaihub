using Microsoft.AspNetCore.Http;
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
    public class ApiDeadLetterConsumer(IServiceScopeFactory scopeFactory,
        IConfiguration configuration,
        IMessageConsumer<ApiRequestDto> consumer,
        ILogger<ApiDeadLetterConsumer> logger,
        IOptions<MessageQueues> queues) : BaseConsumer(configuration)
    {
        private readonly IMessageConsumer<ApiRequestDto> _consumer = consumer;
        private readonly IServiceScopeFactory _scopeFactory = scopeFactory;
        private readonly ILogger<ApiDeadLetterConsumer> _logger = logger;
        private readonly MessageQueues _queues = queues.Value;

        /// <summary>
        /// Executes the background processing logic for handling messages in the dead-letter queue asynchronously.
        /// </summary>
        /// <remarks>This method processes messages from the dead-letter queue and marks associated cards
        /// as failing. It is intended to run continuously until cancellation is requested via the provided
        /// token.</remarks>
        /// <param name="stoppingToken">A cancellation token that can be used to request cancellation of the background operation.</param>
        /// <returns>A task that represents the asynchronous execution of the background operation.</returns>
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var deadLetterQueue = $"{_queues.ApiRequestQueue}.dlq";

            await _consumer.ConsumerAsync(deadLetterQueue, async message =>
            {
                using var scope = _scopeFactory.CreateScope();
                try
                {
                    var connectionString = await GetConnectionStringAsync(scope, message.Tenant!);
                    var httpAccessor = scope.ServiceProvider.GetRequiredService<IHttpContextAccessor>();
                    httpAccessor.HttpContext ??= new DefaultHttpContext();
                    httpAccessor.HttpContext.Items["TenantConnection"] = connectionString;

                    var failingCardService = scope.ServiceProvider.GetRequiredService<IFailingCardService>();
                    await failingCardService.SetFailingCard(message.Data.CardId, message.Email);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error to process Api DeadLetter: {Message}", ex.Message);
                    throw;
                }
            });
        }
    }
}
