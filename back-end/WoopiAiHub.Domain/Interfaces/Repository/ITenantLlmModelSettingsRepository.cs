using WoopiAiHub.Domain.Models;

namespace WoopiAiHub.Domain.Interfaces.Repository
{
    public interface ITenantLlmModelSettingsRepository
    {
        Task<IReadOnlyList<TenantLlmModelSetting>> GetAllAsync();
        Task UpsertAsync(IEnumerable<TenantLlmModelSetting> settings);
        Task DeleteByScopesAsync(IEnumerable<string> scopes);
    }
}
