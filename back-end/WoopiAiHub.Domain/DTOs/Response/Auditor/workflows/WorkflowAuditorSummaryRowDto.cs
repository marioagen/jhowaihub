namespace WoopiAiHub.Domain.DTOs.Response.Auditor.Workflows
{
    public record WorkflowAuditorSummaryRowDto
    {
        public int WorkflowId { get; init; }
        public int DocumentId { get; init; }
        public string WorkflowName { get; init; } = string.Empty;
        public int? TeamId { get; init; }
        public string TeamName { get; init; } = string.Empty;
        public int? ProfileId { get; init; }
        public string ProfileName { get; init; } = string.Empty;
    }
}
