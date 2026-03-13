namespace WoopiAiHub.Domain.DTOs.Response.Auditor
{
    /// <summary>
    /// Composite audit response for a workflow: WorkflowId, WorkflowName, LogCount, StepsCount, CardStatusCount, Cards.
    /// </summary>
    public record WorkflowAuditorDetailsDto
    {
        public int WorkflowId { get; init; }
        public string WorkflowName { get; init; } = string.Empty;
        public int LogCount { get; init; }
        public ICollection<WorkflowAuditorStepCountsDto> StepsCount { get; init; } = [];
        public WorkflowAuditorCardStatusCountDto CardStatusCount { get; init; } = new();
        public ICollection<WorkflowAuditorCardsDto> Cards { get; init; } = [];
    }
}
