using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using WoopiAiHub.Domain.DTOs;
using WoopiAiHub.Domain.DTOs.Messaging;
using WoopiAiHub.Domain.Enum;
using WoopiAiHub.Domain.Interfaces.Messaging;
using WoopiAiHub.Domain.Interfaces.Services;
using WoopiAiHub.Domain.Interfaces.Services.Automation;
using WoopiAiHub.Domain.Utils;
using WoopiAiHub.Infrastructure.Messaging.Configuration;
using WoopiAiHub.Infrastructure.Messaging.Consumers;

namespace WoopiAiHub.Application.Messaging
{
    public class OcrConsumer : BaseConsumer
    {
        private readonly IMessageConsumer<ProcessOcrResultDto> _consumer;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<OcrConsumer> _logger;
        private readonly MessageQueues _queues;

        public OcrConsumer(IServiceScopeFactory scopeFactory,
                           IConfiguration configuration,
                           IMessageConsumer<ProcessOcrResultDto> consumer,
                           ILogger<OcrConsumer> logger,
                           IOptions<MessageQueues> queues) : base(configuration)
        {
            _scopeFactory = scopeFactory;
            _queues = queues.Value;
            _consumer = consumer;
            _logger = logger;
        }

        /// <summary>
        /// Execute the background service to consume messages from the OCR queue.
        /// </summary>
        /// <param name="stoppingToken"></param>
        /// <returns></returns>
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await _consumer.ConsumerAsync(_queues.OcrQueueAiHubResponse, async message =>
            {
                using var scope = _scopeFactory.CreateScope();
                try
                {
                    var connectionString = await GetConnectionStringAsync(scope, message.Tenant);
                    var httpAccessor = scope.ServiceProvider.GetRequiredService<IHttpContextAccessor>();
                    httpAccessor.HttpContext ??= new DefaultHttpContext();
                    httpAccessor.HttpContext.Items["TenantConnection"] = connectionString;

                    var documentPipelineServices = scope.ServiceProvider.GetRequiredService<IDocumentPipelineServices>();
                    var result = await documentPipelineServices.ProcessOcrResult(message);

                    var automationServices = scope.ServiceProvider.GetRequiredService<IAutomationServices>();
                    var usageDailyServices = scope.ServiceProvider.GetRequiredService<IUsageDailyServices>();

                    var pages = message.AnalyzeResult?.Pages?.Count() ?? 0;
                    if (pages == 0)
                        pages = 1;

                    await usageDailyServices.AddByValuesAsync(MetricNames.Page, message.Email, pages, string.Empty, result.WorkflowId);

                    var automationServicesDto = new AutomationServicesDto
                    (
                        result.StepToolId,
                        result.CardId,
                        message.Tenant,
                        message.Email,
                        message.ReferenceFile,
                        0,
                        result.WorkflowId
                    );
                    await automationServices.ContinueExecution(automationServicesDto);
                }
                catch (Exception ex)
                {
                    var documentServices = scope.ServiceProvider.GetRequiredService<IDocumentServices>();
                    await documentServices.ChangeStatusByReferenceFile(message.ReferenceFile,
                                                                       message.Email,
                                                                       DocumentStatus.Failure);

                    _logger.LogError(ex, "Failed to process the answer response.");

                    try
                    {
                        var documentPipelineServices = scope.ServiceProvider.GetRequiredService<IDocumentPipelineServices>();
                        var result = await documentPipelineServices.ProcessOcrResult(message);

                        if (result.CardId > 0)
                        {
                            var failingCardService = scope.ServiceProvider.GetRequiredService<IFailingCardService>();
                            await failingCardService.SetFailingCard(result.CardId, message.Email);
                        }
                    }
                    catch (Exception failingEx)
                    {
                        _logger.LogError(failingEx, "Error marking card as failing after exception in consumer");
                    }
                }
            });
        }
    }
}
