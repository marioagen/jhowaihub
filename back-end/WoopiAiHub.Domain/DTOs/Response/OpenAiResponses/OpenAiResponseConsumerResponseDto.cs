using Newtonsoft.Json.Linq;

namespace WoopiAiHub.Domain.DTOs.Response.OpenAiResponses
{
    public record class OpenAiResponseConsumerResponseDto
    {
        public string ReferenceFile { get; set; } = string.Empty;
        public string Tenant { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public JObject Data { get; set; } = new ();
        public ResponseOpenAiResponseDto Response { get; set; } = new ();
    }
}