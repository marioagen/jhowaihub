namespace WoopiAiHub.Domain.DTOs.Response.Auditor
{
    /// <summary>
    /// Step summary for workflow audit: StepId, StepName, CardCount (distinct cards in that step for the workflow).
    /// </summary>
    public record WorkflowAuditorStepCountsDto
    {
        public int StepId { get; init; }
        public string StepName { get; init; } = string.Empty;
        public int CardCount { get; init; }
    }
}
