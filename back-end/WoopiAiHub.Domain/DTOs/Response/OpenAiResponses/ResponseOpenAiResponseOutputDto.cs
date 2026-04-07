


using System.Text.Json;
using System.Text.Json.Serialization;

namespace WoopiAiHub.Domain.DTOs.Response.OpenAiResponses
{
    public class ResponseOpenAiResponseOutputDto
    {
        public string Id { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public List<ResponseOpenAiResponseOutputMessageContentDto> Content { get; set; } = new();
        public string Output { get; set; } = string.Empty;
        [JsonPropertyName("server_label")]
        public string ServerLabel { get; set; } = string.Empty;
        public string Arguments { get; set; } = string.Empty;
        public JsonElement Tools { get; set; } = new();
    }
}