namespace WoopiAiHub.Domain.DTOs.Response.Auditor
{
    /// <summary>
    /// Audit view item: card id, card name, workflows, actions count, and status name per card.
    /// </summary>
    public record AuditorCardsDto
    {
        public int CardId { get; init; }
        public string CardName { get; init; } = string.Empty;
        public IReadOnlyList<AuditorWorkflowListDto> Workflows { get; init; } = [];
        public int ActionsCount { get; init; }
        public string StatusName { get; init; } = string.Empty;
    }
}
