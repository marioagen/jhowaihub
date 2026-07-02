using WoopiAiHub.Domain.Enum;

namespace WoopiAiHub.Domain.Interfaces.Services
{
    public interface ILlmModelResolver
    {
        Task<string> ResolveModelAsync(string tenantName, LlmModelScope scope, CancellationToken cancellationToken = default);
        Task<string> ResolveApiVersionAsync(LlmModelScope scope, CancellationToken cancellationToken = default);
    }
}
