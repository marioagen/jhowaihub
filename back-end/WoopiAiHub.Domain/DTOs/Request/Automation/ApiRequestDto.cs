using System.Text.Json.Serialization;
using WoopiAiHub.Domain.Utils;

namespace WoopiAiHub.Domain.DTOs.Request.Automation
{
    public record class ApiRequestDto
    {
        public string Url { get; set; } = string.Empty;
        public string Method { get; set; } = string.Empty;
        public Dictionary<string, string>? Query { get; set; }
        public Dictionary<string, string>? Headers { get; set; }
        public string? Body { get; set; }
    }
}
