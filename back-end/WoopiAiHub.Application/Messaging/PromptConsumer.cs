using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.Json;
using WoopiAiHub.Domain.DTOs;
using WoopiAiHub.Domain.DTOs.Messaging;
using WoopiAiHub.Domain.DTOs.Response;
using WoopiAiHub.Domain.Enum;
using WoopiAiHub.Domain.Interfaces.Messaging;
using WoopiAiHub.Domain.Interfaces.Services;
using WoopiAiHub.Domain.Interfaces.Services.Automation;
using WoopiAiHub.Infrastructure.Messaging.Configuration;
using WoopiAiHub.Infrastructure.Messaging.Consumers;

namespace WoopiAiHub.Application.Messaging
{
    public class PromptConsumer : BaseConsumer
    {
        private readonly IMessageConsumer<ChatCompletionResponseDto> _consumer;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<PromptConsumer> _logger;
        private readonly MessageQueues _queues;

        public PromptConsumer(IServiceScopeFactory scopeFactory,
                              IConfiguration configuration,
                              IMessageConsumer<ChatCompletionResponseDto> consumer,
                              ILogger<PromptConsumer> logger,
                              IOptions<MessageQueues> queues) : base(configuration)
        {
            _scopeFactory = scopeFactory;
            _queues = queues.Value;
            _consumer = consumer;
            _logger = logger;
        }

        /// <summary>
        /// Execute the background service to consume messages from the ChatCompletion queue.
        /// </summary>
        /// <param name="stoppingToken"></param>
        /// <returns></returns>
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await _consumer.ConsumerAsync(_queues.ChatCompletionQueueAiHubResponse, async message =>
            {
                using var scope = _scopeFactory.CreateScope();
                try
                {
                    var connectionString = await GetConnectionStringAsync(scope, message.Tenant!);
                    var httpAccessor = scope.ServiceProvider.GetRequiredService<IHttpContextAccessor>();
                    httpAccessor.HttpContext ??= new DefaultHttpContext();
                    httpAccessor.HttpContext.Items["TenantConnection"] = connectionString;

                    var promptServices = scope.ServiceProvider.GetRequiredService<IPromptServices>();
                    await promptServices.ProcessChatCompletionResult(message);

                    var automationServices = scope.ServiceProvider.GetRequiredService<IAutomationServices>();
                    var usageDailyServices = scope.ServiceProvider.GetRequiredService<IUsageDailyServices>();

                    var tokens = message.Usage?.TotalTokens ?? 0;
                    await usageDailyServices.AddByValuesAsync("Prompt", message.Email, tokens);

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
