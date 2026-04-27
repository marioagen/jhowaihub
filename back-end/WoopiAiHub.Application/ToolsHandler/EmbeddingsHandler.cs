using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using WoopiAiHub.Domain.DTOs;
using WoopiAiHub.Domain.DTOs.Messaging;
using WoopiAiHub.Domain.DTOs.Request.Automation;
using WoopiAiHub.Domain.Interfaces.Handlers;
using WoopiAiHub.Domain.Interfaces.Repository.Cache;
using WoopiAiHub.Domain.Models;
using WoopiAiHub.Domain.Utils;
using WoopiAiHub.Infrastructure.Messaging.Configuration;
namespace WoopiAiHub.Application.ToolsHandler;

public class EmbeddingsHandler : IToolHandler
{
    public string Type => HandlersTypes.Embeddings;
    private readonly MessageQueues _messageQueues;
    private readonly ITenantCacheServices _tenantCacheServices;
    private readonly IConfiguration _config;

    public EmbeddingsHandler(ITenantCacheServices tenantCacheServices,
                             IOptions<MessageQueues> messageQueues,
                             IConfiguration config)
    {
        _tenantCacheServices = tenantCacheServices;
        _messageQueues = messageQueues.Value;
        _config = config;
    }

    /// <summary>
    /// Builds an execution payload for processing OCR tasks with multiple outputs from dependent StepTools.
    /// This overload allows handling outputs from multiple dependencies.
    /// </summary>
    /// <param name="automationServicesDto"></param>
    /// <param name="input"></param>
    /// <param name="outputs">Collection of outputs from dependent StepTools</param>
    /// <param name="execution"></param>
    /// <returns></returns>
    public async Task<ExecutionMessageDto> BuildPayload(AutomationServicesDto automationServicesDto,
                                                        StepToolParameter? input,
                                                        ICollection<StepToolOutput> outputs,
                                                        StepToolExecution? execution = null)
    {
        var output = outputs.FirstOrDefault()?.Value ?? string.Empty;
        
        var tenantInfo = await _tenantCacheServices.FindTenantAsync(automationServicesDto.Tenant);
        if (string.IsNullOrEmpty(tenantInfo!.EmbeddingModelName))
        {
            throw new ArgumentException("Embeddings not found");
        }

        var apikey = _config["IndexerApiKey"]!;
        var apiVersion = _config["ChatCompletionSettings:ApiVersion"]!;
        var documents = JsonConvert.DeserializeObject<DocumentEmbeddingsDataDto>(output);
        documents!.ApplicationKey = tenantInfo.AiGatewayKey ?? string.Empty;
        documents.ApplicationId = tenantInfo.AiGatewayApplicationId?.ToString() ?? string.Empty;
        documents.RagProvider = tenantInfo.RagProvider;
        documents.ApiVersion = apiVersion;

        foreach (var item in documents!.DocumentEmbeddings)
        {
            item.KeyMongoAccess = apikey;
            item.EmbeddingModelName = tenantInfo.EmbeddingModelName;
        }

        return new ExecutionMessageDto
        {
            Queue = _messageQueues.EmbeddingQueue,
            Message = documents
        };
    }
}
