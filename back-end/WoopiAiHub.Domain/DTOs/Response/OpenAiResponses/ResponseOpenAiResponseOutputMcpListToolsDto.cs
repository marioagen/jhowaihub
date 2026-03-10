
using System.Text.Json;
using System.Text.Json.Serialization;

namespace WoopiAiHub.Domain.DTOs.Response.OpenAiResponses
{
   public class ResponseOpenAiResponseOutputMcpListToolsDto 
    {
        [JsonPropertyName("server_label")]
        public string ServerLabel { get; set; }
        public JsonElement Tools { get; set; }
    }
}