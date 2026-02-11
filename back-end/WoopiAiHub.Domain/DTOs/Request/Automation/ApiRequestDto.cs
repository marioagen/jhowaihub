using WoopiAiHub.Domain.DTOs.Messaging;

namespace WoopiAiHub.Domain.DTOs.Request.Automation
{
    public record class ApiRequestDto
    {
        public int TemplateId { get; set; }
        public string TemplateName { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public string Method { get; set; } = string.Empty;
        public Dictionary<string, string>? Query { get; set; }
        public Dictionary<string, string>? Headers { get; set; }
        public string? Body { get; set; }
        public MetaDataAutomationDto Data { get; set; }
        public string Tenant { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public int? ExecutionId { get; set; }
        public string ResponseQueue { get; set; } = string.Empty;
        public string? ReferenceFile { get; set; }
    }
}
