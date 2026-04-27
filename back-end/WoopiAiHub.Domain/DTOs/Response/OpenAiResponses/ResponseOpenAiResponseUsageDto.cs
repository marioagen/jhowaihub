using System.Text.Json.Serialization;

namespace WoopiAiHub.Domain.DTOs.Response.OpenAiResponses
{
    public record class ResponseOpenAiResponseUsageDto
    {
        [JsonPropertyName("input_tokens")]
        public int InputTokens { get; set; }
        [JsonPropertyName("output_tokens")]
        public int OutputTokens { get; set; }
        [JsonPropertyName("total_tokens")]
        public int TotalTokens { get; set; }
    }
}