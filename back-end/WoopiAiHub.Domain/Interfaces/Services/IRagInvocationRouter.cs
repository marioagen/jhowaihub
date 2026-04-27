using WoopiAiHub.Domain.DTOs;
using WoopiAiHub.Domain.DTOs.Refit;
using WoopiAiHub.Domain.DTOs.Response;

namespace WoopiAiHub.Domain.Interfaces.Services;

public interface IRagInvocationRouter
{
    Task<CustomQueryExecutionResult> ExecuteCustomQueryAsync(TenantInfoDto tenant,
        string referenceFile,
        string indexerApiKey,
        string emailCreator,
        CustomQueryRequestRefitDto request,
        CancellationToken cancellationToken = default);

    Task DeleteEmbeddingsAsync(TenantInfoDto tenant,
        string referenceFile,
        string indexerApiKey,
        CancellationToken cancellationToken = default);

    Task<ChatCompletionResponseDto> ExecuteChatCompletionAsync(TenantInfoDto tenant,
        string email,
        ChatCompletionDto chatCompletion,
        string model,
        string apiVersion,
        CancellationToken cancellationToken = default);
}
