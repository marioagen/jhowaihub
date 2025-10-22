using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using WoopiAiHub.Application.Utils;
using WoopiAiHub.Domain.DTOs;
using WoopiAiHub.Domain.DTOs.Messaging;
using WoopiAiHub.Domain.DTOs.Request.Automation;
using WoopiAiHub.Domain.Enum;
using WoopiAiHub.Domain.Interfaces.Handlers;
using WoopiAiHub.Domain.Interfaces.Repository.Cache;
using WoopiAiHub.Domain.Interfaces.Services;
using WoopiAiHub.Domain.Models;
using WoopiAiHub.Domain.Utils;
using WoopiAiHub.Infrastructure.Messaging.Configuration;
namespace WoopiAiHub.Application.ToolsHandler;

public class PromptHandler : IToolHandler
{
    public string Type => HandlersTypes.Prompt;
    private readonly MessageQueues _messageQueues;
    private readonly ITenantCacheServices _tenantCacheServices;
    private readonly IPromptServices _promptServices;
    private readonly ChatCompletionSettings _chatCompletionSettings;

    public PromptHandler(ITenantCacheServices tenantCacheServices,
                         IOptions<MessageQueues> messageQueues,
                         IPromptServices promptServices,
                         IOptions<ChatCompletionSettings> chatCompletionSettings)
    {
        _tenantCacheServices = tenantCacheServices;
        _messageQueues = messageQueues.Value;
        _promptServices = promptServices;
        _chatCompletionSettings = chatCompletionSettings.Value;
    }

    public async Task<ExecutionMessageDto> BuildPayload(AutomationServicesDto automationServicesDto,
                                                        StepToolParameter? input,
                                                        string output,
                                                        StepToolExecution? execution = null)
    {

        var promptId = int.Parse(input.Value);
        var promptDto = _promptServices.FindById(promptId);
        var tenantInfo = await _tenantCacheServices.FindTenantAsync(automationServicesDto.Tenant, ColTypeModule.WoopiAiHub);
        var documents = JsonConvert.DeserializeObject<DocumentEmbeddingsDataDto>(output);
        var fullText = string.Join("\n", documents.DocumentEmbeddings.Select(d => d.Text));

        return new ExecutionMessageDto
        {
            Queue = _messageQueues.ChatCompletionQueue,
            Message = new ChatCompletionQueryDto
            {
                ResponseQueue = _messageQueues.ChatCompletionQueueAiHubResponse,
                Data = new MetaDataAutomationDto(automationServicesDto.CardId, automationServicesDto.StepToolId),
                ReferenceFile = automationServicesDto.ReferenceFile,
                Tenant = automationServicesDto.Tenant,
                Model = _chatCompletionSettings.Model,
                ApiVersion = _chatCompletionSettings.ApiVersion,
                ChatCompletion = new ChatCompletionDto
                {
                    Temperature = _chatCompletionSettings.Temperature,
                    MaxTokens = _chatCompletionSettings.MaxTokens,
                    Messages = new List<ChatMessageDto>
                        {
                            new ChatMessageDto
                            {
                                Role = "system",
                                Content = string.Concat("Baseado no: \"", fullText, "\" e seguindo as orientações a seguir: ", promptDto.Text)
                            }
                        }
                },
                Email = automationServicesDto.Email
            }
        };
    }
}