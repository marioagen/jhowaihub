using Refit;
using WoopiAiHub.Domain.DTOs;
using WoopiAiHub.Domain.DTOs.Response;
using WoopiAiHub.Domain.Utils;

namespace WoopiAiHub.Domain.Interfaces.Refit
{
    public interface IOpenAiResponseApi
    {
        [Post("/services/85032382-3b50-4b4c-e757-08de641e689e/openai/v1")]
        Task<ChatCompletionResponseDto> GetResponse([AliasAs("applicationId")] string applicationId,
                                                          [AliasAs("modelName")] string modelName,
                                                          [AliasAs("api-version")][Query] string apiVersion,
                                                          [Header(HeaderNames.ChatCompletionApyKey)] string apiKey,
                                                          [Body] ChatCompletionDto chatCompletionDto);
    }
}
