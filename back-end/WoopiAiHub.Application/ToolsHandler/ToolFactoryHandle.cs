using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using WoopiAiHub.Application.Utils;
using WoopiAiHub.Domain.Interfaces.Handlers;
using WoopiAiHub.Domain.Interfaces.Refit;
using WoopiAiHub.Domain.Interfaces.Repository.Cache;
using WoopiAiHub.Domain.Interfaces.Services;
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
    private readonly IPromptServices _promptServices;
    private readonly IOptions<ChatCompletionSettings> _chatCompletionSettings;

    public ToolFactoryHandler(ITenantCacheServices tenantCacheServices, IOptions<MessageQueues> messageQueues, IKeyGeneratorApi keyGeneratorApi, IConfiguration config, IPromptServices promptServices, IOptions<ChatCompletionSettings> chatCompletionSettings)
    {
        _tenantCacheServices = tenantCacheServices;
        _messageQueues = messageQueues;
        _keyGeneratorApi = keyGeneratorApi;
        _config = config;
        _promptServices = promptServices;
        _chatCompletionSettings = chatCompletionSettings;

    }
    public IToolHandler GetHandler(ToolType type)
    {
        string typeName = type.Name;
        return typeName switch
        {
            HandlersTypes.Ocr => new OcrHandler(_tenantCacheServices, _messageQueues),
            HandlersTypes.Embeddings => new EmbeddingsHandler(_tenantCacheServices, _messageQueues, _keyGeneratorApi, _config),
            HandlersTypes.Prompt => new PromptHandler(_tenantCacheServices, _messageQueues, _promptServices, _chatCompletionSettings),
            _ => throw new ArgumentException($"Handler for type '{typeName}' not found.")
        };
    }
}