using System.Text.Json.Serialization;

namespace WoopiAiHub.Domain.Enum.OpenAiResponses
{
    public enum EResponseOpenAiOutputType
    {
        [JsonPropertyName("message")]
        Message,
        [JsonPropertyName("mcp_list_tools")]
        McpListTools,
        [JsonPropertyName("mcp_call")]
        McpCall
    }
}