using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using WoopiAiHub.Application.Utils;
using WoopiAiHub.Domain.DTOs.Request;
using WoopiAiHub.Domain.DTOs.Response;
using WoopiAiHub.Domain.Enum;
using WoopiAiHub.Domain.Interfaces.Refit;
using WoopiAiHub.Domain.Interfaces.Repository;
using WoopiAiHub.Domain.Interfaces.Repository.Cache;
using WoopiAiHub.Domain.Interfaces.Services;
using WoopiAiHub.Domain.Models;
using WoopiAiHub.Domain.Utils;
using WoopiAiHub.Domain.Utils.ErrorLabels;

namespace WoopiAiHub.Application.Services
{
    public class LlmModelsSettingsServices : ILlmModelsSettingsServices
    {
        private readonly ITenantLlmModelSettingsRepository _settingsRepository;
        private readonly ITenantCacheServices _tenantCacheServices;
        private readonly IModelEmbeddingRepository _modelEmbeddingRepository;
        private readonly IMarketPlaceApi _marketPlaceApi;
        private readonly ILlmModelResolver _llmModelResolver;
        private readonly IConfiguration _configuration;
        private readonly OpenAiSettings _openAiSettings;
        private readonly ChatCompletionSettings _chatCompletionSettings;
        private readonly ILogger<LlmModelsSettingsServices> _logger;

        public LlmModelsSettingsServices(
            ITenantLlmModelSettingsRepository settingsRepository,
            ITenantCacheServices tenantCacheServices,
            IModelEmbeddingRepository modelEmbeddingRepository,
            IMarketPlaceApi marketPlaceApi,
            ILlmModelResolver llmModelResolver,
            IConfiguration configuration,
            IOptions<OpenAiSettings> openAiSettings,
            IOptions<ChatCompletionSettings> chatCompletionSettings,
            ILogger<LlmModelsSettingsServices> logger)
        {
            _settingsRepository = settingsRepository;
            _tenantCacheServices = tenantCacheServices;
            _modelEmbeddingRepository = modelEmbeddingRepository;
            _marketPlaceApi = marketPlaceApi;
            _llmModelResolver = llmModelResolver;
            _configuration = configuration;
            _openAiSettings = openAiSettings.Value;
            _chatCompletionSettings = chatCompletionSettings.Value;
            _logger = logger;
        }

        public async Task<LlmModelsSettingsResponseDto> GetAsync(string tenantName, bool canEdit)
        {
            var availableModels = await GetAvailableModelsAsync(tenantName);
            var effectiveModels = await BuildEffectiveModelsAsync(tenantName);

            return new LlmModelsSettingsResponseDto
            {
                Models = effectiveModels,
                AvailableModels = availableModels.ToList(),
                CanEdit = canEdit,
            };
        }

        public async Task<LlmModelsSettingsResponseDto> UpdateAsync(
            string tenantName,
            string email,
            UpdateLlmModelsSettingsDto request)
        {
            var availableModels = await GetAvailableModelsAsync(tenantName);
            var allowedIds = availableModels.Select(x => x.Id).ToHashSet(StringComparer.OrdinalIgnoreCase);
            var now = DateTime.UtcNow;
            var upserts = new List<TenantLlmModelSetting>();
            var scopesToDelete = new List<string>();

            foreach (var scopeKey in LlmModelScopeKeys.All)
            {
                if (!request.Models.TryGetValue(scopeKey, out var modelName) ||
                    string.IsNullOrWhiteSpace(modelName))
                {
                    scopesToDelete.Add(scopeKey);
                    continue;
                }

                if (!allowedIds.Contains(modelName))
                {
                    throw new AppException(
                        ErrorCode.InvalidValue,
                        $"Model '{modelName}' is not available for scope '{scopeKey}'.",
                        null);
                }

                var systemDefault = await GetSystemDefaultModelAsync(
                    tenantName,
                    LlmModelScopeKeys.FromScopeKey(scopeKey));
                if (string.Equals(systemDefault, modelName, StringComparison.OrdinalIgnoreCase))
                {
                    scopesToDelete.Add(scopeKey);
                    continue;
                }

                upserts.Add(new TenantLlmModelSetting
                {
                    Scope = scopeKey,
                    ModelName = modelName,
                    UpdatedAt = now,
                    UpdatedByEmail = email,
                });
            }

            if (upserts.Count > 0)
            {
                await _settingsRepository.UpsertAsync(upserts);
            }

            if (scopesToDelete.Count > 0)
            {
                await _settingsRepository.DeleteByScopesAsync(scopesToDelete);
            }

            return await GetAsync(tenantName, canEdit: true);
        }

        public async Task<IReadOnlyList<LlmModelOptionDto>> GetAvailableModelsAsync(string tenantName)
        {
            var apiKey = _configuration["KeyAccess"];
            if (!string.IsNullOrWhiteSpace(apiKey) && !string.IsNullOrWhiteSpace(tenantName))
            {
                try
                {
                    var marketplaceModels = await _marketPlaceApi.GetTenantLlmModels(apiKey, tenantName);
                    if (marketplaceModels is { Count: > 0 })
                    {
                        return marketplaceModels
                            .Where(x => !string.IsNullOrWhiteSpace(x.Id))
                            .Select(x => new LlmModelOptionDto
                            {
                                Id = x.Id,
                                Label = string.IsNullOrWhiteSpace(x.Label) ? x.Id : x.Label,
                            })
                            .GroupBy(x => x.Id, StringComparer.OrdinalIgnoreCase)
                            .Select(x => x.First())
                            .OrderBy(x => x.Label)
                            .ToList();
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to load LLM models from Marketplace for tenant {Tenant}", tenantName);
                }
            }

            var embeddings = await _modelEmbeddingRepository.FindAllAsync();
            if (embeddings.Count > 0)
            {
                return embeddings
                    .Select(x => new LlmModelOptionDto
                    {
                        Id = x.Name,
                        Label = FormatModelLabel(x.Name),
                    })
                    .ToList();
            }

            return GetFallbackModels();
        }

        private async Task<Dictionary<string, string>> BuildEffectiveModelsAsync(string tenantName)
        {
            var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            foreach (var scopeKey in LlmModelScopeKeys.All)
            {
                var scope = LlmModelScopeKeys.FromScopeKey(scopeKey);
                result[scopeKey] = await _llmModelResolver.ResolveModelAsync(tenantName, scope);
            }

            return result;
        }

        private async Task<string> GetSystemDefaultModelAsync(string tenantName, LlmModelScope scope)
        {
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

        private static IReadOnlyList<LlmModelOptionDto> GetFallbackModels() =>
        [
            new() { Id = "gpt-4o", Label = "GPT-4o" },
            new() { Id = "gpt-4.1", Label = "GPT-4.1" },
            new() { Id = "gemini-2.5-pro", Label = "Gemini 2.5 Pro" },
            new() { Id = "gemini-flash-latest", Label = "Gemini Flash" },
            new() { Id = "deepseek-r1", Label = "DeepSeek R1" },
            new() { Id = "claude-sonnet", Label = "Claude Sonnet" },
        ];

        private static string FormatModelLabel(string modelName) =>
            modelName switch
            {
                "gpt-4o" => "GPT-4o",
                "gpt-4.1" => "GPT-4.1",
                "gemini-flash-latest" => "Gemini Flash",
                "gemini-2.5-pro" => "Gemini 2.5 Pro",
                "deepseek-chat" => "DeepSeek Chat",
                "deepseek-r1" => "DeepSeek R1",
                "claude-sonnet" => "Claude Sonnet",
                _ => modelName,
            };
    }
}
