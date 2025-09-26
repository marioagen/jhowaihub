using Microsoft.Extensions.Options;
using WoopiAiHub.Domain.Interfaces.Handlers;
using WoopiAiHub.Domain.Interfaces.Repository.Cache;
using WoopiAiHub.Domain.Models;
using WoopiAiHub.Domain.Utils;
using WoopiAiHub.Infrastructure.Messaging.Configuration;
namespace WoopiAiHub.Application.ToolsHandler;
public class ToolFactoryHandler : IToolFactoryHandler 
{
    private readonly IOptions<MessageQueues> _messageQueues;
    private readonly ITenantCacheServices _tenantCacheServices;
    public ToolFactoryHandler(ITenantCacheServices tenantCacheServices, IOptions<MessageQueues> messageQueues)
    {
        _tenantCacheServices = tenantCacheServices;
        _messageQueues = messageQueues;
    }
    public IToolHandler GetHandler(ToolType type)
    {
        string typeName = type.Name;
        return typeName switch
        {
            HandlersTypes.Ocr => new OcrHandler(_tenantCacheServices, _messageQueues),
            HandlersTypes.Embeddings => new EmbeddingsHandler(_tenantCacheServices, _messageQueues),
            _ => throw new ArgumentException($"Handler for type '{typeName}' not found.")
        };
    }    
}