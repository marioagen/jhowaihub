namespace WoopiAiHub.Domain.DTOs.Response.Auditor.Documents
{
    public record DocumentAuditorSummaryDto
    {
        public int DocumentId { get; init; }
        public string DocumentName { get; init; } = string.Empty;
        public IReadOnlyList<DocumentAuditorWorkflowsDto> Workflows { get; init; } = [];
        public int ActionsCount { get; init; }
        public bool IsFinalized { get; init; }
    }
}
