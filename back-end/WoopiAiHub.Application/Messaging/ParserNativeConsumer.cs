using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using WoopiAiHub.Application.Utils;
using WoopiAiHub.Domain.DTOs;
using WoopiAiHub.Domain.DTOs.Messaging;
using WoopiAiHub.Domain.Enum;
using WoopiAiHub.Domain.Interfaces.Messaging;
using WoopiAiHub.Domain.Interfaces.Services;
using WoopiAiHub.Domain.Interfaces.Services.Automation;
using WoopiAiHub.Domain.Interfaces.Utils;
using WoopiAiHub.Domain.Utils;
using WoopiAiHub.Infrastructure.Messaging.Configuration;
using WoopiAiHub.Infrastructure.Messaging.Consumers;

namespace WoopiAiHub.Application.Messaging;

public class ParserNativeConsumer : BaseConsumer
{
    private readonly IMessageConsumer<ProcessOcrDto> _consumer;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<ParserNativeConsumer> _logger;
    private readonly MessageQueues _queues;

    public ParserNativeConsumer(
        IServiceScopeFactory scopeFactory,
        IConfiguration configuration,
        IMessageConsumer<ProcessOcrDto> consumer,
        ILogger<ParserNativeConsumer> logger,
        IOptions<MessageQueues> queues) : base(configuration)
    {
        _scopeFactory = scopeFactory;
        _consumer = consumer;
        _logger = logger;
        _queues = queues.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await _consumer.ConsumerAsync(_queues.ParserNativeQueue, async message =>
        {
            using var scope = _scopeFactory.CreateScope();
            try
            {
                var connectionString = await GetConnectionStringAsync(scope, message.Tenant);
                var httpAccessor = scope.ServiceProvider.GetRequiredService<IHttpContextAccessor>();
                httpAccessor.HttpContext ??= new DefaultHttpContext();
                httpAccessor.HttpContext.Items["TenantConnection"] = connectionString;

                var fileRetriever = scope.ServiceProvider.GetRequiredService<DocumentFileRetriever>();
                var nativeExtractor = scope.ServiceProvider.GetRequiredService<INativePdfTextExtractor>();
                var documentPipelineServices = scope.ServiceProvider.GetRequiredService<IDocumentPipelineServices>();

                var pdfBytes = await fileRetriever.DownloadAsync(message.ReferenceFile, message.Tenant);
                var analyzeResult = nativeExtractor.Extract(pdfBytes);

                var resultDto = new ProcessOcrResultDto
                {
                    Tenant = message.Tenant,
                    ReferenceFile = message.ReferenceFile,
                    Model = message.Model,
                    Email = message.Email,
                    Data = message.Data,
                    AnalyzeResult = analyzeResult
                };

                var result = await documentPipelineServices.ProcessOcrResult(resultDto);

                var automationServices = scope.ServiceProvider.GetRequiredService<IAutomationServices>();
                var usageDailyServices = scope.ServiceProvider.GetRequiredService<IUsageDailyServices>();

                var pages = analyzeResult.Pages?.Count() ?? 0;
                if (pages == 0)
                    pages = 1;

                await usageDailyServices.AddByValuesAsync(
                    MetricNames.Page,
                    message.Email,
                    pages,
                    string.Empty,
                    result.WorkflowId);

                var automationServicesDto = new AutomationServicesDto(
                    result.StepToolId,
                    result.CardId,
                    message.Tenant,
                    message.Email,
                    message.ReferenceFile,
                    0,
                    result.WorkflowId);

                await automationServices.ContinueExecution(automationServicesDto);
            }
            catch (Exception ex)
            {
                var documentServices = scope.ServiceProvider.GetRequiredService<IDocumentServices>();
                await documentServices.ChangeStatusByReferenceFile(
                    message.ReferenceFile,
                    message.Email,
                    DocumentStatus.Failure);

                _logger.LogError(ex, "Failed to process native parser extraction.");

                try
                {
                    if (message.Data.CardId > 0)
                    {
                        var failingCardService = scope.ServiceProvider.GetRequiredService<IFailingCardService>();
                        await failingCardService.SetFailingCard(message.Data.CardId, message.Email);
                    }
                }
                catch (Exception failingEx)
                {
                    _logger.LogError(failingEx, "Error marking card as failing after native parser exception");
                }
            }
        });
    }
}
