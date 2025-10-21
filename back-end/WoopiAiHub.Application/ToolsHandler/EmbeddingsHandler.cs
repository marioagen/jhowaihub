using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using WoopiAiHub.Domain.DTOs;
using WoopiAiHub.Domain.DTOs.Messaging;
using WoopiAiHub.Domain.DTOs.Request.Automation;
using WoopiAiHub.Domain.Enum;
using WoopiAiHub.Domain.Interfaces.Handlers;
using WoopiAiHub.Domain.Interfaces.Refit;
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
    private readonly IKeyGeneratorApi _keyGeneratorApi;
    private readonly IConfiguration _config;
    private const string ConfigKeyAccessName = "keyAccess";

    public EmbeddingsHandler(ITenantCacheServices tenantCacheServices,
                             IOptions<MessageQueues> messageQueues,
                             IKeyGeneratorApi keyGeneratorApi,
                             IConfiguration config)
    {
        _tenantCacheServices = tenantCacheServices;
        _messageQueues = messageQueues.Value;
        _keyGeneratorApi = keyGeneratorApi;
        _config = config;
    }

    /// <summary>
    /// Builds an execution payload for processing OCR tasks based on the provided automation service details.
    /// </summary>
    /// <param name="automationServicesDto"></param>
    /// <param name="input"></param>
    /// <param name="output"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public async Task<ExecutionMessageDto> BuildPayload(AutomationServicesDto automationServicesDto,
                                                        StepToolParameter? input,
                                                        string output,
                                                        StepToolExecution? execution = null)
    {
        var tenantInfo = await _tenantCacheServices.FindTenantAsync(automationServicesDto.Tenant, ColTypeModule.WoopiAiHub);
        if (string.IsNullOrEmpty(tenantInfo!.EmbeddingModelName))
        {
            throw new ArgumentException("Embeddings not found");
        }

        var keyAccess = _config[ConfigKeyAccessName]!;
        var keyMongoAcces = await _keyGeneratorApi.GetKey(keyAccess, automationServicesDto.Tenant);
        var documents = JsonConvert.DeserializeObject<DocumentEmbeddingsDataDto>(output);

        foreach (var item in documents!.DocumentEmbeddings)
        {
            item.KeyMongoAccess = keyMongoAcces;
        }

        return new ExecutionMessageDto
        {
            Queue = _messageQueues.EmbeddingQueue,
            Message = documents
        };
    }
}