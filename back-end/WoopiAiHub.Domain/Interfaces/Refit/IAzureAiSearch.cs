using Refit;
using WoopiAiHub.Domain.DTOs.IntegrationHub;

namespace WoopiAiHub.Domain.Interfaces.Refit;

public interface IAzureAiSearch
{
    [Post("/api/AzureAiSearch/custom-query")]
    Task<HttpResponseMessage> CustomQueryAsync(
        [Header("KeyAccess")] string KeyAccess,
        [Body] IntegrationHubDocumentEmbeddingsQueryRequest request,
        CancellationToken cancellationToken = default);

    [Post("/api/AzureAiSearch/delete")]
    Task<HttpResponseMessage> DeleteEmbeddingsAsync(
        [Header("KeyAccess")] string KeyAccess,
        [Body] IntegrationHubDocumentEmbeddingsDeleteRequest request,
        CancellationToken cancellationToken = default);

    [Post("/api/v1/hub/chat-completion")]
    Task<HttpResponseMessage> ChatCompletionAsync(
        [Header("KeyAccess")] string KeyAccess,
        [Body] IntegrationHubChatCompletionQueryRequest request,
        CancellationToken cancellationToken = default);
}
