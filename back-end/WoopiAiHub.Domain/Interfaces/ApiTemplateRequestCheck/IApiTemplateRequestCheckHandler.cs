using WoopiAiHub.Domain.DTOs.Request;
using WoopiAiHub.Domain.DTOs.Response;

namespace WoopiAiHub.Domain.Interfaces.ApiTemplateRequestCheck
{
    public interface IApiTemplateRequestCheckHandler
    {
        Task<ApiTemplateRequestCheckResponseDto> ExecuteAsync(ApiTemplateRequestCheckRequestDto request, CancellationToken cancellationToken = default);
    }
}
