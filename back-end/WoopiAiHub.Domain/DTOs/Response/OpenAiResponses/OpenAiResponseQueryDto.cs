using WoopiAiHub.Domain.DTOs.Messaging;
using WoopiAiHub.Domain.Enum;

namespace WoopiAiHub.Domain.DTOs.Response.OpenAiResponses
{
    public record class OpenAiResponseQueryDto
    {
        public string ReferenceFile { get; set; } = string.Empty;
        public string Tenant { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public string ApiVersion { get; set; } = string.Empty;
        public string ApplicationId {  get; set; } = string.Empty;
        public string ApplicationKey { get; set; } = string.Empty;
        public string ResponseQueue { get; set; } = string.Empty;
        public LlmProvider? LlmProvider { get; set; }
        public MetaDataAutomationDto Data { get; set; } = new ();
        public ResponseOpenAiRequestDto PromptRequest { get; set; } = new();
    }
}
