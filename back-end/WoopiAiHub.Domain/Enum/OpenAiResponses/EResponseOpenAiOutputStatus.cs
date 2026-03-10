using System.Text.Json.Serialization;

namespace WoopiAiHub.Domain.Enum.OpenAiResponses
{
    public enum EResponseOpenAiOutputStatus
    {
        [JsonPropertyName("validating")]
        Validating,
        [JsonPropertyName("failed")]
        Failed,
        [JsonPropertyName("in_progress")]
        InProgress,
        [JsonPropertyName("finalizing")]
        Finalizing,
        [JsonPropertyName("completed")]
        Completed,
        [JsonPropertyName("expired")]
        Expired,
        [JsonPropertyName("cancelling")]
        Cancelling,
        [JsonPropertyName("cancelled")]
        Cancelled,
    }
}


