namespace WoopiAiHub.Domain.DTOs.Response.Auditor
{
    /// <summary>
    /// Step summary for workflow audit: StepId, StepName, DocumentCount (distinct documents in that step for the workflow).
    /// </summary>
    public record WorkflowAuditorStepCountsDto
    {
        public int StepId { get; init; }
        public string StepName { get; init; } = string.Empty;
        public int DocumentCount { get; init; }
    }
}
