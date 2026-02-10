using WoopiAiHub.Domain.DTOs.Messaging;

namespace WoopiAiHub.Domain.DTOs.Request.Automation
{
    public record class AutomationInputDto
    {
        public string Url { get; set; } = string.Empty;
        public string WebhookId { get; set; } = string.Empty;
        public bool RequiredFile { get; set; }
        public string? Tenant { get; set; }
        public string? Email { get; set; }
        public int ExecutionId { get; set; }
        public string? ReferenceFile { get; set; }
        public string ResponseQueue { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public MetaDataAutomationDto Data { get; set; }
        public object? Content { get; set; }
    }
}
