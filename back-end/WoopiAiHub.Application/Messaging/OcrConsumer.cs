using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using WoopiAiHub.Domain.DTOs.Response;
using WoopiAiHub.Domain.Enum;
using WoopiAiHub.Domain.Interfaces.Messaging;
using WoopiAiHub.Domain.Interfaces.Services;
using WoopiAiHub.Infrastructure.Messaging.Configuration;
using WoopiAiHub.Infrastructure.Messaging.Consumers;

namespace WoopiAiHub.Application.Messaging
{
    public class OcrConsumer : BaseConsumer
    {
        private readonly IMessageConsumer<ProcessOcrResultDto> _consumer;
        private readonly IMessagePublisher<IEnumerable<DocumentEmbeddingsAddDto>> _publisher;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<OcrConsumer> _logger;
        private readonly MessageQueues _queues;

        public OcrConsumer(IServiceScopeFactory scopeFactory,
                           IConfiguration configuration,
                           IMessageConsumer<ProcessOcrResultDto> consumer,
                           IMessagePublisher<IEnumerable<DocumentEmbeddingsAddDto>> publisher,
                           ILogger<OcrConsumer> logger,
                           IOptions<MessageQueues> queues) : base(scopeFactory, configuration)
        {
            _scopeFactory = scopeFactory;
            _queues = queues.Value;
            _consumer = consumer;
            _publisher = publisher;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await _consumer.ConsumerAsync(_queues.AnswerQueueAiHubResponse, async message =>
            {
                using var scope = _scopeFactory.CreateScope();

                try
                {
                    await ConfigureTenantContextAsync(message.Tenant, ColTypeModule.WoopiAiHub);

                    var documentServices = scope.ServiceProvider.GetRequiredService<IDocumentServices>();
                    var result = await documentServices.ProcessOcrResult(message);

                    await _publisher.PublishAsync(_queues.EmbeddingQueue, result);
                }
                catch (Exception ex)
                {
                    var documentServices = scope.ServiceProvider.GetRequiredService<IDocumentServices>();
                    await documentServices.ChangeStatusByReferenceFile(message.ReferenceFile,
                                                                       message.Email,
                                                                       DocumentStatus.Failure);

                    _logger.LogError(ex, "Failed to process the answer response.");
                }
            });
        }
    }
}
