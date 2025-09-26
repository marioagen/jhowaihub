using WoopiAiHub.Domain.Interfaces.Handlers;
using WoopiAiHub.Domain.DTOs.Request.Automation;
using WoopiAiHub.Domain.Interfaces.Repository.Cache;
using WoopiAiHub.Domain.Enum;
using Newtonsoft.Json;
using WoopiAiHub.Domain.DTOs.Messaging;
using WoopiAiHub.Infrastructure.Messaging.Configuration;
using Microsoft.Extensions.Options;
namespace WoopiAiHub.Application.ToolsHandler;

public class EmbeddingsHandler : IToolHandler
{
    private readonly MessageQueues _messageQueues;
    private readonly ITenantCacheServices _tenantCacheServices;
    public EmbeddingsHandler(ITenantCacheServices tenantCacheServices, IOptions<MessageQueues> messageQueues)
    {
        _tenantCacheServices = tenantCacheServices;
        _messageQueues = messageQueues.Value;
    }
    public async Task<ExecutionMessageDto> BuildPayload(string tenant, string referenceFile, string input, int stepToolId, int cardId)
    {
        var tenantInfo = await _tenantCacheServices.FindTenantAsync(tenant, ColTypeModule.WoopiAiHub);
        if (string.IsNullOrEmpty(tenantInfo!.EmbeddingModelName))
        {
            throw new ArgumentException("Embeddings not found");
        }

        return new ExecutionMessageDto
        {
            Queue = _messageQueues.EmbeddingQueue,
            Message = JsonConvert.SerializeObject(new ProcessOcrDto
            {
                Data = new MetaDataAutomationDto(cardId, stepToolId),
                Tenant = tenant,
                ReferenceFile = referenceFile,
                Model = tenantInfo.EmbeddingModelName,
                Email = "",
                ResponseQueue = _messageQueues.EmbeddingQueueAiHubResponse
            })
        };
    }
}