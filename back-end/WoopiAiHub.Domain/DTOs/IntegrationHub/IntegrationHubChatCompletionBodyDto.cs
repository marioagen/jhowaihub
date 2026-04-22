using Newtonsoft.Json;

namespace WoopiAiHub.Domain.DTOs.IntegrationHub;

public record class IntegrationHubChatCompletionBodyDto
{
    public List<IntegrationHubChatMessageDto> Messages { get; set; } = [];

    public double Temperature { get; set; }

    [JsonProperty("max_tokens")]
    public int MaxTokens { get; set; }

    public bool Stream { get; set; }
}
