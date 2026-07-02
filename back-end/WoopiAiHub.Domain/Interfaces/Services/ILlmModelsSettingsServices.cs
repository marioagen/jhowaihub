using WoopiAiHub.Domain.DTOs.Request;
using WoopiAiHub.Domain.DTOs.Response;

namespace WoopiAiHub.Domain.Interfaces.Services
{
    public interface ILlmModelsSettingsServices
    {
        Task<LlmModelsSettingsResponseDto> GetAsync(string tenantName, bool canEdit);
        Task<LlmModelsSettingsResponseDto> UpdateAsync(
            string tenantName,
            string email,
            UpdateLlmModelsSettingsDto request);
        Task<IReadOnlyList<LlmModelOptionDto>> GetAvailableModelsAsync(string tenantName);
    }
}
