using WoopiAiHub.Domain.DTOs.Request;
using WoopiAiHub.Domain.DTOs.Response;

namespace WoopiAiHub.Domain.Interfaces.ApiTemplateRequestTests
{
    public interface IApiTemplateRequestTestsHandler
    {
        Task<ApiTemplateRequestTestsResponseDto> ExecuteAsync(ApiTemplateRequestTestsRequestDto request, CancellationToken cancellationToken = default);
    }
}
