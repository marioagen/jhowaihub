using System.Net;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using WoopiAiHub.Application.Utils;
using WoopiAiHub.Domain.DTOs;
using WoopiAiHub.Domain.Enum;
using WoopiAiHub.Domain.DTOs.IntegrationHub;
using WoopiAiHub.Domain.DTOs.Refit;
using WoopiAiHub.Domain.DTOs.Response;
using WoopiAiHub.Domain.Interfaces.Refit;
using WoopiAiHub.Domain.Interfaces.Services;
using WoopiAiHub.Domain.Utils;

namespace WoopiAiHub.Application.Services;

public sealed class RagInvocationRouter : IRagInvocationRouter
{
    private static readonly JsonSerializerSettings JsonSettings = new()
    {
        ContractResolver = new CamelCasePropertyNamesContractResolver(),
        NullValueHandling = NullValueHandling.Ignore
    };

    private readonly IIntegrationHubApi _integrationHubApi;
    private readonly IEmbeddingsApi _embeddingsApi;
    private readonly IChatCompletionApi _chatCompletionApi;
    private readonly IConfiguration _configuration;
    private readonly ILogger<RagInvocationRouter> _logger;

    public RagInvocationRouter(IIntegrationHubApi integrationHubApi,
        IEmbeddingsApi embeddingsApi,
        IChatCompletionApi chatCompletionApi,
        IConfiguration configuration,
        ILogger<RagInvocationRouter> logger)
    {
        _integrationHubApi = integrationHubApi;
        _embeddingsApi = embeddingsApi;
        _chatCompletionApi = chatCompletionApi;
        _configuration = configuration;
        _logger = logger;
    }

    /// <summary>
    /// Runs a custom query against the Indexer Refit client or the integration-services HTTP API depending on tenant RAG provider.
    /// </summary>
    public async Task<CustomQueryExecutionResult> ExecuteCustomQueryAsync(TenantInfoDto tenant,
        string referenceFile,
        string indexerApiKey,
        string emailCreator,
        CustomQueryRequestRefitDto request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(tenant);
        if (RoutesToIntegration(tenant))
        {
            return await ExecuteIntegrationCustomQueryAsync(tenant, referenceFile, indexerApiKey, emailCreator, request,
                cancellationToken).ConfigureAwait(false);
        }

        return await ExecuteIndexerCustomQueryAsync(tenant.Name, referenceFile, indexerApiKey, request,
            cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Deletes embeddings via the Indexer Refit client or the integration-services HTTP API depending on tenant RAG provider.
    /// </summary>
    public async Task DeleteEmbeddingsAsync(TenantInfoDto tenant,
        string referenceFile,
        string indexerApiKey,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(tenant);
        if (RoutesToIntegration(tenant))
        {
            await ExecuteIntegrationDeleteAsync(tenant, referenceFile, indexerApiKey, cancellationToken)
                .ConfigureAwait(false);
            return;
        }

        await ExecuteIndexerDeleteAsync(tenant.Name, referenceFile, indexerApiKey, cancellationToken)
            .ConfigureAwait(false);
    }

    /// <summary>
    /// Executes chat completion via the AI Gateway Refit client or the integration-services HTTP API depending on tenant RAG provider.
    /// </summary>
    public async Task<ChatCompletionResponseDto> ExecuteChatCompletionAsync(TenantInfoDto tenant,
        string email,
        ChatCompletionDto chatCompletion,
        string model,
        string apiVersion,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(tenant);
        if (tenant.AiGatewayApplicationId is null || string.IsNullOrEmpty(tenant.AiGatewayKey))
        {
            throw new ArgumentException("AiGateway ApplicationId or key not found for tenant.");
        }

        if (RoutesToIntegration(tenant))
        {
            return await ExecuteIntegrationChatCompletionAsync(tenant, email, chatCompletion, model, apiVersion,
                cancellationToken).ConfigureAwait(false);
        }

        return await _chatCompletionApi
            .GetChatCompletion(tenant.AiGatewayApplicationId.Value.ToString(), model, apiVersion, tenant.AiGatewayKey,
                chatCompletion).ConfigureAwait(false);
    }

    private static bool RoutesToIntegration(TenantInfoDto tenant)
    {
        var p = tenant.RagProvider?.Trim();
        return !string.IsNullOrEmpty(p) &&
               p.Equals(RagProviderNames.AzureAiSearch, StringComparison.OrdinalIgnoreCase);
    }

    private async Task<CustomQueryExecutionResult> ExecuteIndexerCustomQueryAsync(string tenantName,
        string referenceFile,
        string indexerApiKey,
        CustomQueryRequestRefitDto request,
        CancellationToken cancellationToken)
    {
        var httpResponse = await _embeddingsApi.CustomQuery(tenantName, referenceFile, request, indexerApiKey)
            .ConfigureAwait(false);
        var body = await httpResponse.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        if (!httpResponse.IsSuccessStatusCode)
        {
            if (httpResponse.StatusCode == HttpStatusCode.NotFound)
            {
                throw new FileNotFoundException("The file was not found in the llmindexer weaviate");
            }

            throw new AppException(ErrorCode.RefitApiError, "Error while sending question to Embeddings API", null);
        }

        var model = JsonConvert.DeserializeObject<QueryResponseModelRefitDto>(body);
        if (model?.response == null)
        {
            throw new InvalidOperationException("Embeddings API custom query response value is null.");
        }

        var usage = model.Usage ?? [];
        return new CustomQueryExecutionResult(model.response, usage.ToList());
    }

    private async Task<CustomQueryExecutionResult> ExecuteIntegrationCustomQueryAsync(TenantInfoDto tenant,
        string referenceFile,
        string indexerApiKey,
        string emailCreator,
        CustomQueryRequestRefitDto request,
        CancellationToken cancellationToken)
    {
        var apiVersion = _configuration["ChatCompletionSettings:ApiVersion"] ?? string.Empty;
        var payload = new IntegrationHubDocumentEmbeddingsQueryRequest
        {
            RagProvider = tenant.RagProvider,
            ApplicationId = tenant.AiGatewayApplicationId?.ToString() ?? string.Empty,
            ApplicationKey = tenant.AiGatewayKey,
            ApiVersion = apiVersion,
            EmbeddingModelName = tenant.EmbeddingModelName,
            ReferenceFile = referenceFile,
            KeyMongoAccess = indexerApiKey,
            Questions =
            [
                new IntegrationHubQuestionDto
                {
                    Id = 0,
                    Question = request.Question
                }
            ],
            kValue = request.kValue,
            Model = request.Model,
            Template = request.Template,
            Temperature = request.Temperature,
            Refine_template = request.Refine_template,
            Max_tokens = request.Max_tokens,
            SearchMode = request.SearchMode,
            Tenant = tenant.Name,
            Email = emailCreator,
            Data = new Newtonsoft.Json.Linq.JObject()
        };

        var keyAccess = _configuration["KeyAccess"] ?? string.Empty;
        using var response =
            await _integrationHubApi.CustomQueryAsync(keyAccess, payload, cancellationToken).ConfigureAwait(false);
        var responseBody = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("Integration custom-query failed: {Status} {Body}", response.StatusCode, responseBody);
            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                throw new FileNotFoundException("The file was not found in the llmindexer weaviate");
            }

            throw new AppException(ErrorCode.RefitApiError, "Error while sending question to Integration API", null);
        }

        var parsed =
            JsonConvert.DeserializeObject<IntegrationHubDocumentEmbeddingsQueryResponse>(responseBody, JsonSettings);
        var first = parsed?.QuestionsAnswers?.FirstOrDefault();
        if (first == null || string.IsNullOrEmpty(first.Answer))
        {
            throw new InvalidOperationException("Integration API custom query returned no answer.");
        }

        var mappedUsage = (first.Usage ?? []).Select(u => new QueryUsageDto
        {
            Model = u.Model,
            Usage_unity = string.Empty,
            Prompt_usage = null,
            Completion_usage = null,
            Total_usage = u.Total_usage
        }).ToList();

        return new CustomQueryExecutionResult(first.Answer, mappedUsage);
    }

    private async Task ExecuteIndexerDeleteAsync(string tenantName,
        string referenceFile,
        string indexerApiKey,
        CancellationToken cancellationToken)
    {
        var resultRequest =
            await _embeddingsApi.DeleteHash(tenantName, referenceFile, tenantName, indexerApiKey)
                .ConfigureAwait(false);
        if (!resultRequest.IsSuccessStatusCode && resultRequest.StatusCode != HttpStatusCode.NotFound)
        {
            throw new ArgumentException("Error while sending delete hash in Embeddings API");
        }
    }

    private async Task ExecuteIntegrationDeleteAsync(TenantInfoDto tenant,
        string referenceFile,
        string indexerApiKey,
        CancellationToken cancellationToken)
    {
        var payload = new IntegrationHubDocumentEmbeddingsDeleteRequest
        {
            RagProvider = tenant.RagProvider,
            ReferenceFile = referenceFile,
            KeyMongoAccess = indexerApiKey,
            Tenant = tenant.Name
        };

        var keyAccess = _configuration["KeyAccess"] ?? string.Empty;
        using var response =
            await _integrationHubApi.DeleteEmbeddingsAsync(keyAccess, payload, cancellationToken).ConfigureAwait(false);
        if (!response.IsSuccessStatusCode && response.StatusCode != HttpStatusCode.NotFound)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
            _logger.LogWarning("Integration delete failed: {Status} {Body}", response.StatusCode, body);
            throw new ArgumentException("Error while sending delete to Integration API");
        }
    }

    private async Task<ChatCompletionResponseDto> ExecuteIntegrationChatCompletionAsync(TenantInfoDto tenant,
        string email,
        ChatCompletionDto chatCompletion,
        string model,
        string apiVersion,
        CancellationToken cancellationToken)
    {
        var bodyDto = new IntegrationHubChatCompletionBodyDto
        {
            Temperature = chatCompletion.Temperature,
            MaxTokens = chatCompletion.MaxTokens,
            Stream = chatCompletion.Stream,
            Messages = chatCompletion.Messages
                .Select(m => new IntegrationHubChatMessageDto { Role = m.Role, Content = m.Content }).ToList()
        };

        var payload = new IntegrationHubChatCompletionQueryRequest
        {
            ReferenceFile = string.Empty,
            Tenant = tenant.Name,
            Email = email,
            Model = model,
            ApiVersion = apiVersion,
            ApplicationId = tenant.AiGatewayApplicationId!.Value.ToString(),
            ApplicationKey = tenant.AiGatewayKey,
            ResponseQueue = string.Empty,
            Data = new Newtonsoft.Json.Linq.JObject(),
            ChatCompletion = bodyDto
        };

        var keyAccess = _configuration["KeyAccess"] ?? string.Empty;
        using var response =
            await _integrationHubApi.ChatCompletionAsync(keyAccess, payload, cancellationToken).ConfigureAwait(false);
        var responseBody = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("Integration chat-completion failed: {Status} {Body}", response.StatusCode,
                responseBody);
            throw new AppException(ErrorCode.RefitApiError, "Error while calling Integration chat-completion API", null);
        }

        var result = JsonConvert.DeserializeObject<ChatCompletionResponseDto>(responseBody, JsonSettings);
        if (result == null)
        {
            throw new InvalidOperationException("Integration chat-completion returned an empty body.");
        }

        return result;
    }
}
