

using System.Text.Json.Serialization;

namespace WoopiAiHub.Domain.DTOs.Response.OpenAiResponses
{
    public class ResponseOpenAiResponseOutputMessageDto 
{
    [JsonPropertyName("content")]
    public List<ResponseOpenAiResponseOutputMessageContentDto> Content { get; set; } = new();
}
}