namespace WoopiAiHub.Domain.DTOs.Response.Auditor
{
    public record WorkflowAuditorStepCountsDto
    {
        public int StepId { get; init; }
        public string StepName { get; init; } = string.Empty;
        public int DocumentCount { get; init; }
    }
}
