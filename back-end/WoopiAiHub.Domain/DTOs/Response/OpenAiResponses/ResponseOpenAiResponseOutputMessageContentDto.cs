

using System.Text.Json.Serialization;

namespace WoopiAiHub.Domain.DTOs.Response.OpenAiResponses
{
    public class ResponseOpenAiResponseOutputMessageContentDto 
    {
        [JsonPropertyName("type")]
        public string Type { get; set; } = default!;   // "output_text", etc.

        [JsonPropertyName("text")]
        public string? Text { get; set; }

    }
}