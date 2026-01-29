using System.Text.Json.Serialization;

namespace WoopiAiHub.Domain.DTOs
{
    public record ChatCompletionDto
    {
        public List<ChatMessageDto> Messages { get; set; } = new();
        public double Temperature { get; set; }
        [JsonPropertyName("max_tokens")]
        public int MaxTokens { get; set; }
        public bool Stream { get; set; } = false;
    }
}
