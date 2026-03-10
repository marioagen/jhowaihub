
using System.Text.Json.Serialization;
namespace WoopiAiHub.Domain.DTOs.Response.OpenAiResponses
{
    public record class ResponseOpenAiRequestInputDto
    {
        [JsonPropertyName("type")]
        public string Type { get; set; }
        [JsonPropertyName("role")]
        public string Role { get; set; }
        [JsonPropertyName("content")]
        public List<ResponseOpenAiRequestInputContentDto> Content { get; set; }
    }

    public record class ResponseOpenAiRequestInputContentDto
    {
        [JsonPropertyName("type")]
        public string Type { get; set; }
        [JsonPropertyName("text")]
        public string Text { get; set; }
    }
}