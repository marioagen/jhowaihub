using System.Text.Json.Serialization;

namespace WoopiAiHub.Domain.DTOs.Response.OpenAiResponses
{
    public record class ResponseOpenAiRequestDto
    {
        [JsonPropertyName("temperature")]
        public double Temperature { get; set; }
        [JsonPropertyName("max_tool_calls")]
        public int MaxToolCalls { get; set; }
        [JsonPropertyName("model")]
        public string Model { get; set; } = string.Empty;
        [JsonPropertyName("instructions")]
        public string Instructions { get; set; } = string.Empty;
        [JsonPropertyName("tools")]
        public List<ResponseOpenAiRequestToolsDto> Tools { get; set; } = new ();
        [JsonPropertyName("input")]
        public List<ResponseOpenAiRequestInputDto> Input { get; set; } = new ();
    }
}