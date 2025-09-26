using WoopiAiHub.Domain.Interfaces.Handlers;
using WoopiAiHub.Domain.DTOs.Request.Automation;
using WoopiAiHub.Domain.Interfaces.Repository.Cache;
using WoopiAiHub.Domain.Enum;
using Newtonsoft.Json;
using WoopiAiHub.Domain.DTOs.Messaging;
using WoopiAiHub.Infrastructure.Messaging.Configuration;
using Microsoft.Extensions.Options;
using System.Collections.ObjectModel;
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
    public async Task<ExecutionMessageDto> BuildPayload(string tenant, 
                                                        string referenceFile,
                                                        string input, 
                                                        int stepToolId, 
                                                        int cardId,
                                                        string email)
    {
        var tenantInfo = await _tenantCacheServices.FindTenantAsync(tenant, ColTypeModule.WoopiAiHub);
        if (string.IsNullOrEmpty(tenantInfo!.EmbeddingModelName))
        {
            throw new ArgumentException("Embeddings not found");
        }

        return new ExecutionMessageDto
        {
            Queue = _messageQueues.EmbeddingQueue,
            Message = new DocumentEmbeddingsDataDto
            {
                Data = new MetaDataAutomationDto(cardId, stepToolId),
                DocumentEmbeddings = new Collection<DocumentEmbeddingsAddDto>()
                {
                    new DocumentEmbeddingsAddDto
                    {
                        Tenant = tenant,
                        Email = email,
                        Text = "teste",
                        ReferenceFile = referenceFile,
                        KeyMongoAccess = "YhI2fEXWmu4UKIW48UR5UXdXhLWoJ6Sq7Gr6FLGWvzo=",
                        EmbeddingModelName = "text-embedding-3-large",
                        ChunkSize = 4096,
                        Metadata = new { PageNumber = 1 },
                    }
                },
                ReferenceFile = referenceFile,
                ResponseQueue = _messageQueues.EmbeddingQueueAiHubResponse,

            }
        };
    }
}