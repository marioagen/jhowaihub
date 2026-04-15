using System.Text.Json.Serialization;

namespace WoopiAiHub.Domain.DTOs.Response.OpenAiResponses
{
    public record class ResponseOpenAiResponseDto
    {
        [JsonPropertyName("usage")]
        public ResponseOpenAiResponseUsageDto Usage { get; set; } = new();

        [JsonPropertyName("output")]
        public List<ResponseOpenAiResponseOutputDto> Output { get; set; } = new();
    }
}