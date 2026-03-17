namespace WoopiAiHub.Domain.DTOs.Response.Auditor.Rows
{
    /// <summary>
    /// Raw audit row for workflow summary. Used by repository and service to build WorkflowAuditorSummaryDto.
    /// </summary>
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
