namespace WoopiAiHub.Domain.DTOs.Request
{
    public record class WebhookInputDto
    {
        public int ToolId { get; set; }
        public Guid workflowId { get; set; }
    }
}
