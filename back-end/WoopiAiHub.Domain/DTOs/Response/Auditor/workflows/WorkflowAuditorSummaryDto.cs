namespace WoopiAiHub.Domain.DTOs.Response.Auditor
{
    public record WorkflowAuditorSummaryDto
    {
        public int WorkflowId { get; init; }
        public string WorkflowName { get; init; } = string.Empty;
        public int DocumentCount { get; init; }
        public int LogsCount { get; init; }
        public int? TeamId { get; init; }
        public string TeamName { get; init; } = string.Empty;
        public int? ProfileId { get; init; }
        public string ProfileName { get; init; } = string.Empty;
    }
}
