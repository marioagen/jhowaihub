using Refit;
using WoopiAiHub.Domain.DTOs;
using WoopiAiHub.Domain.DTOs.Response;
using WoopiAiHub.Domain.Utils;

namespace WoopiAiHub.Domain.Interfaces.Refit
{
    public interface IChatCompletionApi
    {
        [Post("/services/{applicationId}/openai/deployments/{modelName}/chat/completions")]
        Task<ChatCompletionResponseDto> GetChatCompletion([AliasAs("applicationId")] string applicationId,
                                                          [AliasAs("modelName")] string modelName,
                                                          [AliasAs("api-version")][Query] string apiVersion,
                                                          [Header(HeaderNames.ChatCompletionApyKey)] string apiKey,
                                                          [Body] ChatCompletionDto chatCompletionDto);
    }
}
