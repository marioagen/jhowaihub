using Microsoft.Extensions.Options;
using WoopiAiHub.Application.Utils;
using WoopiAiHub.Domain.Enum;
using WoopiAiHub.Domain.Interfaces.Repository;
using WoopiAiHub.Domain.Interfaces.Repository.Cache;
using WoopiAiHub.Domain.Interfaces.Services;
using WoopiAiHub.Domain.Utils;

namespace WoopiAiHub.Application.Services
{
    public class LlmModelResolver : ILlmModelResolver
    {
        private readonly ITenantCacheServices _tenantCacheServices;
        private readonly ITenantLlmModelSettingsRepository _settingsRepository;
        private readonly OpenAiSettings _openAiSettings;
        private readonly ChatCompletionSettings _chatCompletionSettings;

        public LlmModelResolver(
            ITenantCacheServices tenantCacheServices,
            ITenantLlmModelSettingsRepository settingsRepository,
            IOptions<OpenAiSettings> openAiSettings,
            IOptions<ChatCompletionSettings> chatCompletionSettings)
        {
            _tenantCacheServices = tenantCacheServices;
            _settingsRepository = settingsRepository;
            _openAiSettings = openAiSettings.Value;
            _chatCompletionSettings = chatCompletionSettings.Value;
        }

        public async Task<string> ResolveModelAsync(
            string tenantName,
            LlmModelScope scope,
            CancellationToken cancellationToken = default)
        {
            var scopeKey = LlmModelScopeKeys.ToScopeKey(scope);
            var storedSettings = await _settingsRepository.GetAllAsync();
            var overrideModel = storedSettings
                .FirstOrDefault(x => x.Scope == scopeKey)
                ?.ModelName;

            if (!string.IsNullOrWhiteSpace(overrideModel))
            {
                return overrideModel;
            }

            if (scope == LlmModelScope.Chat)
            {
                return _chatCompletionSettings.Model;
            }

            var tenantInfo = await _tenantCacheServices.FindTenantAsync(tenantName);
            if (tenantInfo is null)
            {
                return _openAiSettings.Model;
            }

            return tenantInfo.LlmProvider == LlmProvider.AzureOpenAI
                ? _openAiSettings.Model
                : tenantInfo.Model;
        }

        public Task<string> ResolveApiVersionAsync(
            LlmModelScope scope,
            CancellationToken cancellationToken = default)
        {
            var apiVersion = scope == LlmModelScope.Chat
                ? _chatCompletionSettings.ApiVersion
                : _openAiSettings.ApiVersion;

            return Task.FromResult(apiVersion);
        }
    }
}
