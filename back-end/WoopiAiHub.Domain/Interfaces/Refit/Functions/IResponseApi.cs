using Refit;
using WoopiAiHub.Domain.DTOs.Response.OpenAiResponses;
using WoopiAiHub.Domain.Utils;

namespace WoopiAiHub.Domain.Interfaces.Refit.Functions
{
    public interface IResponseApi
    {
        [Post("/services/{applicationId}/openai/deployments/{modelName}/v1/responses")]
        Task<ResponseOpenAiResponseDto> GetResponseOpenAi([AliasAs("applicationId")] string applicationId,
                                                          [AliasAs("modelName")] string modelName,
                                                        //   [AliasAs("api-version")] string apiVersion,
                                                          [Header("x-session-id")] string sessionId,
                                                          [Header(HeaderNames.ResponseApiKey)] string apiKey,
                                                          [Body] ResponseOpenAiRequestDto requestDto);
    }
}