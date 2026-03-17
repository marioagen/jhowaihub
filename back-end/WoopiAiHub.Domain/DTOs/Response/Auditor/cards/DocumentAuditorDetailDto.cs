namespace WoopiAiHub.Domain.DTOs.Response.Auditor
{
    /// <summary>
    /// Document audit detail: DocumentId, DocumentName, WorkflowId, WorkflowName, and DocumentHistory (list of audit entries).
    /// </summary>
    public record DocumentAuditorDetailDto
    {
        public int DocumentId { get; init; }
        public string DocumentName { get; init; } = string.Empty;
        public int WorkflowId { get; init; }
        public string WorkflowName { get; init; } = string.Empty;
        public IReadOnlyList<DocumentAuditorHistoryEntryDto> DocumentHistory { get; init; } = [];
    }
}
