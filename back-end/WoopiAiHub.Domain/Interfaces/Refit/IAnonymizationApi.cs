using Refit;
using WoopiAiHub.Domain.DTOs.Refit;

namespace WoopiAiHub.Domain.Interfaces.Refit
{
    public interface IAnonymizationApi
    {
        [Post("/api/Anonimization")]
        Task<AnonymizationResponseDto> InitiateAnonymization(
            [Header("Authorization")] string authorization,
            [Body] AnonymizationRequestDto request
        );
    }
}
