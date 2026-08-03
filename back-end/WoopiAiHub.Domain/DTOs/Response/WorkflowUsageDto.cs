namespace WoopiAiHub.Domain.DTOs.Response
{
    public record WorkflowUsageDto
    {
        public int WorkflowId { get; set; }
        public string WorkflowName { get; set; } = string.Empty;
    }
}
