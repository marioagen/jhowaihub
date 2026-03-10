

using System.Text.Json.Serialization;

namespace WoopiAiHub.Domain.DTOs.Response.OpenAiResponses
{
    public record class ResponseOpenAiRequestToolsDto
    {
        [JsonPropertyName("type")]
        public string Type { get; set; }
        [JsonPropertyName("server_label")]
        public string ServerLabel { get; set; }
        [JsonPropertyName("server_url")]
        public string ServerUrl { get; set; }
        [JsonPropertyName("headers")]
        public Dictionary<string, string> Headers { get; set; }
        [JsonPropertyName("require_approval")]
        public string RequireApproval { get; set; }
        [JsonPropertyName("allowed_tools")]
        public List<string> AllowedTools { get; set; } = null;        
    }
}