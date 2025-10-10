using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using WoopiAiHub.Domain.Interfaces.Handlers;
using WoopiAiHub.Domain.Interfaces.Refit;
using WoopiAiHub.Domain.Interfaces.Repository;
using WoopiAiHub.Domain.Interfaces.Repository.Cache;
using WoopiAiHub.Domain.Models;
using WoopiAiHub.Domain.Utils;
using WoopiAiHub.Infrastructure.Messaging.Configuration;

namespace WoopiAiHub.Application.ToolsHandler;

public class ToolFactoryHandler : IToolFactoryHandler
{
    private readonly IOptions<MessageQueues> _messageQueues;
    private readonly ITenantCacheServices _tenantCacheServices;
    private readonly IKeyGeneratorApi _keyGeneratorApi;
    private readonly IConfiguration _config;
    private readonly IToolRepository _toolRepository;

    public ToolFactoryHandler(ITenantCacheServices tenantCacheServices,
                              IToolRepository toolRepository,
                              IOptions<MessageQueues> messageQueues, 
                              IKeyGeneratorApi keyGeneratorApi, 
                              IConfiguration config)
    {
        _tenantCacheServices = tenantCacheServices;
        _toolRepository = toolRepository;
        _messageQueues = messageQueues;
        _keyGeneratorApi = keyGeneratorApi;
        _config = config;
    }
    public IToolHandler GetHandler(ToolType type)
    {
        string typeName = type.Name;
        return typeName switch
        {
            HandlersTypes.Ocr => new OcrHandler(_tenantCacheServices, _messageQueues),
            HandlersTypes.Embeddings => new EmbeddingsHandler(_tenantCacheServices, _messageQueues, _keyGeneratorApi, _config),
            HandlersTypes.N8N => new N8NHandler(_messageQueues, _toolRepository),
            _ => throw new ArgumentException($"Handler for type '{typeName}' not found.")
        };
    }
}