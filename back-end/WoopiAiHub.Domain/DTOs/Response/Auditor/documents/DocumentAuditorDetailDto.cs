namespace WoopiAiHub.Domain.DTOs.Response.Auditor.Documents
{
    public record DocumentAuditorDetailDto
    {
        public int DocumentId { get; init; }
        public string DocumentName { get; init; } = string.Empty;
        public int WorkflowId { get; init; }
        public string WorkflowName { get; init; } = string.Empty;
        public IReadOnlyList<DocumentAuditorHistoryEntryDto> DocumentHistory { get; init; } = [];
    }
}
