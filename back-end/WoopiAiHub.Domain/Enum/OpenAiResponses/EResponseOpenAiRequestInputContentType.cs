using System.Text.Json.Serialization;

namespace WoopiAiHub.Domain.Enum.OpenAiResponses
{
    public enum EResponseOpenAiRequestInputContentType
    {
        [JsonPropertyName("input_text")] 
        InputText,
        [JsonPropertyName("output_text")] 
        OutputText
    }
}