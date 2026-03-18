namespace WoopiAiHub.Domain.DTOs.Response.Auditor
{
    public record WorkflowAuditorDetailsDto
    {
        public int WorkflowId { get; init; }
        public string WorkflowName { get; init; } = string.Empty;
        public int LogCount { get; init; }
        public ICollection<WorkflowAuditorStepCountsDto> StepsCount { get; init; } = [];
        public WorkflowAuditorDocumentStatusCountDto DocumentStatusCount { get; init; } = new();
        public ICollection<WorkflowAuditorCardsDto> Cards { get; init; } = [];
    }
}
