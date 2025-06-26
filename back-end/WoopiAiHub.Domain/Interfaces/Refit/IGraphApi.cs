using WoopiAiHub.Domain.DTOs;
using WoopiAiHub.Domain.DTOs.Refit;
using Refit;

namespace WoopiAiHub.Domain.Interfaces.Refit
{
    public interface IGraphApi
    {
        [Get("/v1.0/me")]
        Task<ApiResponse<UserGraphApiResponse>> FindEmailUserAzure([Header("Authorization")] string authorization);
    }
}
