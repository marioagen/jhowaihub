using System.Text.Json.Serialization;
namespace WoopiAiHub.Domain.DTOs.Response.OpenAiResponses
{
    public record class ResponseOpenAiRequestInputDto
    {
        [JsonPropertyName("type")]
        public string Type { get; set; } = string.Empty;
        [JsonPropertyName("role")]
        public string Role { get; set; } = string.Empty;
        [JsonPropertyName("content")]
        public List<ResponseOpenAiRequestInputContentDto> Content { get; set; } = new ();
    }
}