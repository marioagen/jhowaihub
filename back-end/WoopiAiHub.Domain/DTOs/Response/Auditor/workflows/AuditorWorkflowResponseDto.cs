namespace WoopiAiHub.Domain.DTOs.Response.Auditor
{
    /// <summary>
    /// Composite audit response for a workflow: WorkflowId, WorkflowName, LogCount, StepsCount, CardStatusCount, Cards.
    /// </summary>
    public record AuditorWorkflowResponseDto
    {
        public int WorkflowId { get; init; }
        public string WorkflowName { get; init; } = string.Empty;
        public int LogCount { get; init; }
        public ICollection<StepsCountResponseDto> StepsCount { get; init; } = [];
        public WorkflowAuditCardStatusCountResponseDto CardStatusCount { get; init; } = new();
        public ICollection<WorkflowAuditCardResponseDto> Cards { get; init; } = [];
    }
}
