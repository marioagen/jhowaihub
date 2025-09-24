using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using WoopiAiHub.Domain.DTOs.Messaging;
using WoopiAiHub.Domain.Enum;
using WoopiAiHub.Domain.Interfaces.Messaging;
using WoopiAiHub.Domain.Interfaces.Services;
using WoopiAiHub.Domain.Interfaces.Services.Automation;
using WoopiAiHub.Infrastructure.Messaging.Configuration;
using WoopiAiHub.Infrastructure.Messaging.Consumers;
using static Google.Cloud.Vision.V1.ProductSearchResults.Types;

namespace WoopiAiHub.Application.Messaging
{
    public class DocumentEmbeddingsConsumer : BaseConsumer
    {
        private readonly IMessageConsumer<DocumentEmbeddingsResultDto> _consumer;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<DocumentEmbeddingsConsumer> _logger;
        private readonly MessageQueues _queues;

        public DocumentEmbeddingsConsumer(IServiceScopeFactory scopeFactory,
                           IConfiguration configuration,
                           IMessageConsumer<DocumentEmbeddingsResultDto> consumer,
                           ILogger<DocumentEmbeddingsConsumer> logger,
                           IOptions<MessageQueues> queues) : base(configuration)
        {
            _scopeFactory = scopeFactory;
            _queues = queues.Value;
            _consumer = consumer;
            _logger = logger;
        }

        /// <summary>
        /// Execute the background embeddings of document 
        /// </summary>
        /// <param name="stoppingToken"></param>
        /// <returns></returns>
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await _consumer.ConsumerAsync(_queues.EmbeddingQueueAiHubResponse, async message =>
            {
                using var scope = _scopeFactory.CreateScope();
                try
                {
                    var connectionString = await GetConnectionStringAsync(scope, message.Tenant!, ColTypeModule.WoopiAiHub);
                    var httpAccessor = scope.ServiceProvider.GetRequiredService<IHttpContextAccessor>();
                    httpAccessor.HttpContext ??= new DefaultHttpContext();
                    httpAccessor.HttpContext.Items["TenantConnection"] = connectionString;

                    var documentServices = scope.ServiceProvider.GetRequiredService<IDocumentServices>();
                    var result = await documentServices.ProcessEmbeddingsResult(message);

                    var automationServices = scope.ServiceProvider.GetRequiredService<IAutomationServices>();
                    await automationServices.ContinueExecution(result.StepToolId, result.CardId);
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
