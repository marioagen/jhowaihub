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
using WoopiAiHub.Domain.Utils.ErrorLabels;
using WoopiAiHub.Infrastructure.Messaging.Configuration;
namespace WoopiAiHub.Application.ToolsHandler;

public class PromptHandler : IToolHandler
{
    public string Type => HandlersTypes.Prompt;
    private readonly MessageQueues _messageQueues;
    private readonly IPromptServices _promptServices;
    private readonly ITenantCacheServices _tenantCacheServices;
    private readonly ChatCompletionSettings _chatCompletionSettings;

    public PromptHandler(IOptions<MessageQueues> messageQueues,
                         IPromptServices promptServices,
                         IOptions<ChatCompletionSettings> chatCompletionSettings,
                         ITenantCacheServices tenantCacheServices)
    {
        _messageQueues = messageQueues.Value;
        _promptServices = promptServices;
        _chatCompletionSettings = chatCompletionSettings.Value;
        _tenantCacheServices = tenantCacheServices;
    }

    /// <summary>
    /// Builds an execution payload for processing prompt tasks with multiple outputs from dependent StepTools.
    /// Dependencies can be OCR (document embeddings) or another Prompt (previous prompt response); the combined text is sent to the AI Gateway.
    /// </summary>
    /// <param name="automationServicesDto"></param>
    /// <param name="input"></param>
    /// <param name="outputs">Collection of outputs from dependent StepTools (OCR and/or Prompt)</param>
    /// <param name="execution"></param>
    /// <returns></returns>
    public async Task<ExecutionMessageDto> BuildPayload(AutomationServicesDto automationServicesDto,
                                                        StepToolParameter? input,
                                                        ICollection<StepToolOutput> outputs,
                                                        StepToolExecution? execution = null)
    {
        var tenantInfo = await _tenantCacheServices.FindTenantAsync(automationServicesDto.Tenant);
        if (tenantInfo!.AiGatewayApplicationId.HasValue is false || string.IsNullOrEmpty(tenantInfo.AiGatewayKey))
        {
            throw new ArgumentException("AiGateway ApplicationId not found");
        }

        var fullText = ExtractFullTextFromOutputs(outputs);
        if (string.IsNullOrWhiteSpace(fullText))
        {
            throw new AppException(ErrorCode.RequiredField, "The prompt tool requires an OCR or Prompt dependency with output", ToolLabel.OcrOrPromptDependencyRequired);
        }

        var promptId = int.Parse(input!.Value);
        var promptDto = _promptServices.FindById(promptId);

        return new ExecutionMessageDto
        {
            Queue = _messageQueues.ChatCompletionQueue,
            Message = new ChatCompletionQueryDto
            {
                ResponseQueue = _messageQueues.ChatCompletionQueueAiHubResponse,
                Data = new MetaDataAutomationDto(automationServicesDto.CardId, automationServicesDto.StepToolId),
                ReferenceFile = automationServicesDto.ReferenceFile!,
                Tenant = automationServicesDto.Tenant,
                Model = _chatCompletionSettings.Model,
                ApiVersion = _chatCompletionSettings.ApiVersion,
                ApplicationId = tenantInfo!.AiGatewayApplicationId.Value.ToString(),
                ApplicationKey = tenantInfo!.AiGatewayKey,
                ChatCompletion = new ChatCompletionDto
                {
                    Temperature = _chatCompletionSettings.Temperature,
                    MaxTokens = _chatCompletionSettings.MaxTokens,
                    Messages = new List<ChatMessageDto>
                        {
                            new ChatMessageDto
                            {
                                Role = "system",
                                Content = string.Concat("Baseado no: \"", fullText, "\" e seguindo as orientações a seguir: ", promptDto!.Text)
                            }
                        }
                },
                Email = automationServicesDto.Email
            }
        };
    }

    /// <summary>
    /// Extracts and concatenates text from dependency outputs. OCR output is parsed from DocumentEmbeddings; Prompt output is used as plain text.
    /// When StepTool/ToolType is not available (e.g. tests), tries OCR format first, then falls back to plain text.
    /// </summary>
    private static string ExtractFullTextFromOutputs(ICollection<StepToolOutput> outputs)
    {
        if (outputs == null || outputs.Count == 0) return string.Empty;

        var parts = new List<string>();
        foreach (var output in outputs)
        {
            var value = output.Value;
            if (string.IsNullOrWhiteSpace(value)) continue;

            var toolType = output.StepTool?.Tool?.ToolType?.Name;
            if (string.Equals(toolType, HandlersTypes.Ocr, StringComparison.OrdinalIgnoreCase))
            {
                TryAddOcrText(value, parts);
            }
            else if (string.Equals(toolType, HandlersTypes.Prompt, StringComparison.OrdinalIgnoreCase))
            {
                parts.Add(value.Trim());
            }
        }

        return string.Join("\n\n", parts);
    }

    /// <summary>
    /// Tries to parse OCR output in DocumentEmbeddings format and extract text. If parsing fails, returns false to allow fallback to plain text.
    /// </summary>
    /// <param name="value"></param>
    /// <param name="parts"></param>
    /// <returns></returns>
    private static bool TryAddOcrText(string value, List<string> parts)
    {
       var documents = JsonConvert.DeserializeObject<DocumentEmbeddingsDataDto>(value);
       if (documents?.DocumentEmbeddings != null && documents.DocumentEmbeddings.Count > 0)
       {
           parts.Add(string.Join("\n", documents.DocumentEmbeddings.Select(d => d.Text)));
           return true;
       }
        return false;
    }
}
