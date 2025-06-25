using DocAnalyzer.Domain.DTOs;
using DocAnalyzer.Domain.DTOs.Refit;
using Refit;

namespace DocAnalyzer.Domain.Interfaces.Refit
{
    public interface IGraphApi
    {
        [Get("/v1.0/me")]
        Task<ApiResponse<UserGraphApiResponse>> FindEmailUserAzure([Header("Authorization")] string authorization);
    }
}
