using Microsoft.Extensions.Options;
using WoopiAiHub.Application.Utils;
using WoopiAiHub.Domain.DTOs;
using WoopiAiHub.Domain.Enum;
using WoopiAiHub.Domain.Interfaces.Refit;
using WoopiAiHub.Domain.Interfaces.Repository.Cache;
using WoopiAiHub.Domain.Interfaces.Services;
using WoopiAiHub.Domain.Utils;
using WoopiAiHub.Infrastructure.Messaging.Configuration;

namespace WoopiAiHub.Application.Services
{
    public class PlaygroundServices : IPlaygroundServices
    {
        private readonly ITenantCacheServices _tenantCacheServices;
        private readonly IChatCompletionApi _chatCompletionApi;
        private readonly ChatCompletionSettings _chatCompletionSettings;
        private readonly IUsageDailyServices _usageDailyServices;

        public PlaygroundServices(
            ITenantCacheServices tenantCacheServices,
            IChatCompletionApi chatCompletionApi,
            IOptions<ChatCompletionSettings> chatCompletionSettings,
            IUsageDailyServices usageDailyServices)
        {
            _tenantCacheServices = tenantCacheServices;
            _chatCompletionApi = chatCompletionApi;
            _chatCompletionSettings = chatCompletionSettings.Value;
            _usageDailyServices = usageDailyServices;
        }

        /// <summary>
        /// Runs a synchronous chat completion with the same system message shape as the automation Prompt tool (context + instructions), logs token usage, and does not persist anything.
        /// </summary>
        public async Task<string> TestPromptWithContextAsync(string promptText, string contextText, string tenantId, string email)
        {
            ArgumentNullException.ThrowIfNull(promptText, "Prompt is null");
            ArgumentNullException.ThrowIfNull(contextText, "Context is null");

            if (string.IsNullOrWhiteSpace(promptText))
            {
                throw new ArgumentException("Prompt text is required");
            }

            var tenantInfo = await _tenantCacheServices.FindTenantAsync(tenantId);
            if (tenantInfo!.AiGatewayApplicationId.HasValue is false || string.IsNullOrEmpty(tenantInfo.AiGatewayKey))
            {
                throw new ArgumentException("AiGateway ApplicationId not found");
            }

            var fullText = contextText ?? string.Empty;
            var systemContent = string.Concat("Baseado no: \"", fullText, "\" e seguindo as orientações a seguir: ", promptText);

            var chatCompletionDto = new ChatCompletionDto
            {
                Temperature = _chatCompletionSettings.Temperature,
                MaxTokens = _chatCompletionSettings.MaxTokens,
                Messages = new List<ChatMessageDto>
                {
                    new ChatMessageDto { Role = "system", Content = systemContent }
                }
            };

            var response = await _chatCompletionApi.GetChatCompletion(
                tenantInfo.AiGatewayApplicationId.Value.ToString(),
                _chatCompletionSettings.Model,
                _chatCompletionSettings.ApiVersion,
                tenantInfo.AiGatewayKey,
                chatCompletionDto);

            var tokens = response.Usage?.TotalTokens ?? 0;
            await _usageDailyServices.AddByValuesAsync(MetricNames.Token, email, tokens, _chatCompletionSettings.Model,
                workflowId: null, UsageDailyOrigin.Playground);

            var content = response.Choices.FirstOrDefault()?.Message?.Content;
            if (string.IsNullOrEmpty(content))
            {
                throw new AppException(ErrorCode.DefaultError, "Empty response from AI Gateway", null);
            }

            return content;
        }
    }
}
