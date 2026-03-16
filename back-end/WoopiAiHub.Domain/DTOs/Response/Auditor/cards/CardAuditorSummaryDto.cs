namespace WoopiAiHub.Domain.DTOs.Response.Auditor
{
    /// <summary>
    /// Audit view item: card id, card name, workflows, actions count, and isFinalized per card.
    /// </summary>
    public record CardAuditorSummaryDto
    {
        public int CardId { get; init; }
        public string CardName { get; init; } = string.Empty;
        public IReadOnlyList<CardAuditorWorkflowsDto> Workflows { get; init; } = [];
        public int ActionsCount { get; init; }
        public bool IsFinalized { get; init; }
    }
}
