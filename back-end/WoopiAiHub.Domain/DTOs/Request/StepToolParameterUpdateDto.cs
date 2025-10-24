namespace WoopiAiHub.Domain.DTOs.Request
{
    public record class StepToolParameterUpdateDto
    {
        public int StepToolId { get; set; }
        public string Value { get; set; } = string.Empty;      
        public bool RequiredFile { get; set; }
        public Guid? WebhookId { get; set; }
    }
}
