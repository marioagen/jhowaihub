namespace WoopiAiHub.Domain.DTOs.Response.Auditor
{
    /// <summary>
    /// One audit row for a card: CardId, CardName, Created, WorkflowId, WorkflowName, UserId, UserName, ActionName, StepId, StepName.
    /// </summary>
    public record CardAuditorDetailDto
    {
        public int CardId { get; init; }
        public string CardName { get; init; } = string.Empty;
        public DateTime Created { get; init; }
        public int WorkflowId { get; init; }
        public string WorkflowName { get; init; } = string.Empty;
        public Guid UserId { get; init; }
        public string UserName { get; init; } = string.Empty;
        public string ActionName { get; init; } = string.Empty;
        public int StepId { get; init; }
        public string StepName { get; init; } = string.Empty;
    }
}
