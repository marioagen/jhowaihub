
using System.Text.Json.Serialization;

namespace WoopiAiHub.Domain.DTOs.Response.OpenAiResponses
{
    public class ResponseOpenAiResponseOutputMcpCallDto : ResponseOpenAiResponseOutputDto
    {
        public string Output { get; set; }
        [JsonPropertyName("server_label")]
        public string ServerLabel { get; set; }
        public string Arguments { get; set; }
    }
}