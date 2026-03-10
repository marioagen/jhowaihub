
using System.Text.Json.Serialization;

namespace WoopiAiHub.Domain.Enum.OpenAiResponses
{
    public enum EResponseOpenAiRequestInputRole
    {
        [JsonPropertyName("system")]
        System,
        [JsonPropertyName("developer")]
        Developer,
        [JsonPropertyName("user")]
        User,
        [JsonPropertyName("assistent")]
        Assistent
    }
}