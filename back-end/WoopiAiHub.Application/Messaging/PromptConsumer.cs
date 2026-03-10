using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.Json;
using WoopiAiHub.Application.Utils;
using WoopiAiHub.Domain.DTOs;
using WoopiAiHub.Domain.DTOs.Messaging;
using WoopiAiHub.Domain.DTOs.Response.OpenAiResponses;
using WoopiAiHub.Domain.Enum;
using WoopiAiHub.Domain.Interfaces.Messaging;
using WoopiAiHub.Domain.Interfaces.Services;
using WoopiAiHub.Domain.Interfaces.Services.Automation;
using WoopiAiHub.Domain.Utils;
using WoopiAiHub.Infrastructure.Messaging.Configuration;
using WoopiAiHub.Infrastructure.Messaging.Consumers;

namespace WoopiAiHub.Application.Messaging
{
    public class PromptConsumer : BaseConsumer
    {
        private readonly IMessageConsumer<OpenAiResponseConsumerResponseDto> _consumer;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<PromptConsumer> _logger;
        private readonly MessageQueues _queues;
        private readonly ResponseOpenAiSettings _responseOpenAiSettings;

        public PromptConsumer(IServiceScopeFactory scopeFactory,
                              IConfiguration configuration,
                              IMessageConsumer<OpenAiResponseConsumerResponseDto> consumer,
                              ILogger<PromptConsumer> logger,
                              IOptions<MessageQueues> queues,
                              IOptions<ResponseOpenAiSettings> responseOpenAiSettings) : base(configuration)
        {
            _scopeFactory = scopeFactory;
            _queues = queues.Value;
            _consumer = consumer;
            _logger = logger;
            _responseOpenAiSettings = responseOpenAiSettings.Value;
        }

        /// <summary>
        /// Execute the background service to consume messages from the ChatCompletion queue.
        /// </summary>
        /// <param name="stoppingToken"></param>
        /// <returns></returns>
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await _consumer.ConsumerAsync(_queues.OpenAiResponseQueueAiHubResponse, async message =>
            {
                using var scope = _scopeFactory.CreateScope();
                try
                {
                    var connectionString = await GetConnectionStringAsync(scope, message.Tenant!);
                    var httpAccessor = scope.ServiceProvider.GetRequiredService<IHttpContextAccessor>();
                    httpAccessor.HttpContext ??= new DefaultHttpContext();
                    httpAccessor.HttpContext.Items["TenantConnection"] = connectionString;

                    var promptServices = scope.ServiceProvider.GetRequiredService<IPromptServices>();
                    // await promptServices.ProcessChatCompletionResult(message);
                    await promptServices.ProcessOpenAiResponseResult(message);

                    var automationServices = scope.ServiceProvider.GetRequiredService<IAutomationServices>();
                    var usageDailyServices = scope.ServiceProvider.GetRequiredService<IUsageDailyServices>();

                    var tokens = message.Response.Usage?.TotalTokens ?? 0;
                    await usageDailyServices.AddByValuesAsync(MetricNames.Token, message.Email, tokens, _responseOpenAiSettings.Model);

                    var dataDto = JsonSerializer.Deserialize<MetaDataAutomationDto>(message.Data.ToString());
                    var automationServicesDto = new AutomationServicesDto
                    (
                        dataDto.StepToolId,
                        dataDto.CardId,
                        message.Tenant,
                        message.Email,
                        message.ReferenceFile,
                        0
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
                }
            });

        }
    }
}
