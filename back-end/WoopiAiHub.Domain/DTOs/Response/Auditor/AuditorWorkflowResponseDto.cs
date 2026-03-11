namespace WoopiAiHub.Domain.DTOs.Response.Auditor
{
    /// <summary>
    /// Workflow-based audit entry: one row per workflow with card count, logs count, team and profile.
    /// </summary>
    public record AuditorWorkflowResponseDto
    {
        public int WorkflowId { get; init; }
        public string WorkflowName { get; init; } = string.Empty;
        public int CardCount { get; init; }
        public int LogsCount { get; init; }
        public int? TeamId { get; init; }
        public string TeamName { get; init; } = string.Empty;
        public int? ProfileId { get; init; }
        public string ProfileName { get; init; } = string.Empty;
    }
}
