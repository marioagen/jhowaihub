namespace WoopiAiHub.Domain.DTOs.Response.Auditor
{
    /// <summary>
    /// Audit view item: document id, document name, workflows (with step), actions count, and isFinalized (all cards of document finalized).
    /// </summary>
    public record CardAuditorSummaryDto
    {
        public int DocumentId { get; init; }
        public string DocumentName { get; init; } = string.Empty;
        public IReadOnlyList<CardAuditorWorkflowsDto> Workflows { get; init; } = [];
        public int ActionsCount { get; init; }
        public bool IsFinalized { get; init; }
    }
}
