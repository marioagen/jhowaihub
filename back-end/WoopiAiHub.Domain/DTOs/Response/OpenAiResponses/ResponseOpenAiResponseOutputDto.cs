


using System.Text.Json;
using System.Text.Json.Serialization;

namespace WoopiAiHub.Domain.DTOs.Response.OpenAiResponses
{
    public class ResponseOpenAiResponseOutputDto
    {
        public string Id { get; set; }
        public string Type { get; set; }
        public string Status { get; set; }
        public List<ResponseOpenAiResponseOutputMessageContentDto> Content { get; set; }
        public string Output { get; set; }
        [JsonPropertyName("server_label")]
        public string ServerLabel { get; set; }
        public string Arguments { get; set; }
        public JsonElement Tools { get; set; }
    }
}