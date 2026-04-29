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

    private readonly IAzureAiSearch _integrationHubApi;
    private readonly IEmbeddingsApi _embeddingsApi;
    private readonly IChatCompletionApi _chatCompletionApi;
    private readonly IConfiguration _configuration;
    private readonly ILogger<RagInvocationRouter> _logger;

    public RagInvocationRouter(IAzureAiSearch integrationHubApi,
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
            return await ExecuteIntegrationCustomQueryAsync(tenant, referenceFile, indexerApiKey, emailCreator, request, cancellationToken);
        }

        return await ExecuteIndexerCustomQueryAsync(tenant.Name, referenceFile, indexerApiKey, request, cancellationToken);
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
            await ExecuteIntegrationDeleteAsync(tenant, referenceFile, indexerApiKey, cancellationToken);
            return;
        }

        await ExecuteIndexerDeleteAsync(tenant.Name, referenceFile, indexerApiKey);
    }

    /// <summary>
    /// Executes chat completion via the AI Gateway Refit client 
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

        return await _chatCompletionApi
            .GetChatCompletion(tenant.AiGatewayApplicationId.Value.ToString(), model, apiVersion, tenant.AiGatewayKey, chatCompletion);
    }

    /// <summary>
    /// Tenant routes to integration if RagProvider is set to Azure AI Search, which means that instead of calling the Indexer Refit client, the service will call the integration-services HTTP API for custom query and delete operations. This is needed because when using Azure AI Search as RAG provider, the embeddings are stored in a separate system than when using other RAG providers, so the flow for querying and deleting embeddings needs to be different.
    /// </summary>
    /// <param name="tenant"></param>
    /// <returns></returns>
    private static bool RoutesToIntegration(TenantInfoDto tenant)
        => tenant.RagProvider == RagProvider.AzureAiSearch;

    /// <summary>
    /// Executes a custom query against the indexer service for the specified tenant and reference file asynchronously.
    /// </summary>
    /// <param name="tenantName">The name of the tenant for which the custom query is executed. Cannot be null or empty.</param>
    /// <param name="referenceFile">The identifier of the reference file to query within the indexer. Cannot be null or empty.</param>
    /// <param name="indexerApiKey">The API key used to authenticate the request to the indexer service. Cannot be null or empty.</param>
    /// <param name="request">The custom query request details to be sent to the indexer API. Cannot be null.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the execution result of the custom
    /// query, including the response and usage information.</returns>
    /// <exception cref="FileNotFoundException">Thrown if the specified reference file is not found in the indexer service.</exception>
    /// <exception cref="AppException">Thrown if an error occurs while sending the query to the Embeddings API.</exception>
    /// <exception cref="InvalidOperationException">Thrown if the response from the Embeddings API does not contain a valid result.</exception>

    private async Task<CustomQueryExecutionResult> ExecuteIndexerCustomQueryAsync(string tenantName,
        string referenceFile,
        string indexerApiKey,
        CustomQueryRequestRefitDto request,
        CancellationToken cancellationToken)
    {
        var httpResponse = await _embeddingsApi.CustomQuery(tenantName, referenceFile, request, indexerApiKey);
        var body = await httpResponse.Content.ReadAsStringAsync(cancellationToken);
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

    /// <summary>
    /// Execute Integrataion Custom Query when tenant RAG provider is set to Azure AI Search, executes the custom query via the integration-services HTTP API.
    /// </summary>
    /// <param name="tenant"></param>
    /// <param name="referenceFile"></param>
    /// <param name="indexerApiKey"></param>
    /// <param name="emailCreator"></param>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <exception cref="FileNotFoundException"></exception>
    /// <exception cref="AppException"></exception>
    /// <exception cref="InvalidOperationException"></exception>
    private async Task<CustomQueryExecutionResult> ExecuteIntegrationCustomQueryAsync(TenantInfoDto tenant,
        string referenceFile,
        string indexerApiKey,
        string emailCreator,
        CustomQueryRequestRefitDto request,
        CancellationToken cancellationToken)
    {
        var apiVersion = _configuration["ChatCompletionSettings:ApiVersion"] ?? string.Empty;
        var payload = CreateRequestQuery(tenant, referenceFile, indexerApiKey, emailCreator, request, apiVersion);

        var keyAccess = _configuration["KeyAccess"] ?? string.Empty;
        using var response = await _integrationHubApi.CustomQueryAsync(keyAccess, payload, cancellationToken);
        var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
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

    /// <summary>
    /// Creates a new IntegrationHubDocumentEmbeddingsQueryRequest using the specified tenant information, reference
    /// file, API key, creator email, query request details, and API version.
    /// </summary>
    /// <param name="tenant">The tenant information used to populate provider, application, and model details. Cannot be null.</param>
    /// <param name="referenceFile">The name or path of the reference file to associate with the query. Cannot be null or empty.</param>
    /// <param name="indexerApiKey">The API key used for accessing the indexer. Cannot be null or empty.</param>
    /// <param name="emailCreator">The email address of the user creating the request. Cannot be null or empty.</param>
    /// <param name="request">The query request details, including the question, model parameters, and search options. Cannot be null.</param>
    /// <param name="apiVersion">The API version to use for the request. Cannot be null or empty.</param>
    /// <returns>An IntegrationHubDocumentEmbeddingsQueryRequest populated with the provided parameters and ready for submission.</returns>
    private static IntegrationHubDocumentEmbeddingsQueryRequest CreateRequestQuery(
        TenantInfoDto tenant,
        string referenceFile,
        string indexerApiKey,
        string emailCreator,
        CustomQueryRequestRefitDto request,
        string apiVersion)
    {
        return new IntegrationHubDocumentEmbeddingsQueryRequest
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
    }

    /// <summary>
    /// Executes the delete operation for the Azure AI Search integration by calling the Indexer Refit client. It will throw an exception if the response is not successful to indicate that something went wrong with the request, but it will not throw if the status code is NotFound to avoid breaking the flow in case the file was not found in the indexer and therefore there is nothing to delete.
    /// </summary>
    /// <param name="tenantName"></param>
    /// <param name="referenceFile"></param>
    /// <param name="indexerApiKey"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    private async Task ExecuteIndexerDeleteAsync(string tenantName,
        string referenceFile,
        string indexerApiKey)
    {
        var resultRequest =
            await _embeddingsApi.DeleteHash(tenantName, referenceFile, tenantName, indexerApiKey);
        if (!resultRequest.IsSuccessStatusCode && resultRequest.StatusCode != HttpStatusCode.NotFound)
        {
            throw new ArgumentException("Error while sending delete hash in Embeddings API");
        }
    }

    /// <summary>
    /// Executes the delete operation for the Azure AI Search integration by calling the Integration Hub API. It logs a warning instead of throwing if the response is not successful to avoid breaking the flow in case of issues with the Integration API, but it will throw an exception if the status code is different than NotFound to indicate that something went wrong with the request.
    /// </summary>
    /// <param name="tenant"></param>
    /// <param name="referenceFile"></param>
    /// <param name="indexerApiKey"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
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
            await _integrationHubApi.DeleteEmbeddingsAsync(keyAccess, payload, cancellationToken);
        if (!response.IsSuccessStatusCode && response.StatusCode != HttpStatusCode.NotFound)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogError("Integration delete failed: {Status} {Body}", response.StatusCode, body);
            throw new ArgumentException("Error while sending delete to Integration API");
        }
    }
}
