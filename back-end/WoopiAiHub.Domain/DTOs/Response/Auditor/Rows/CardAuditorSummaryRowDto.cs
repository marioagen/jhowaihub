namespace WoopiAiHub.Domain.DTOs.Response.Auditor.Rows
{
    /// <summary>
    /// Raw audit row for cards summary. Used by repository and service to build CardAuditorSummaryDto.
    /// </summary>
    public record CardAuditorSummaryRowDto
    {
        public int DocumentId { get; init; }
        public string DocumentName { get; init; } = string.Empty;
        public int WorkflowId { get; init; }
        public string WorkflowName { get; init; } = string.Empty;
        public int StepId { get; init; }
        public string StepName { get; init; } = string.Empty;
        public int CardId { get; init; }
        public string CardStatusName { get; init; } = string.Empty;
    }
}
