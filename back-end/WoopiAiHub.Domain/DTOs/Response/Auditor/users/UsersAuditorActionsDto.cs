namespace WoopiAiHub.Domain.DTOs.Response.Auditor
{
    /// <summary>
    /// One audit log entry for a user: CardId, CardName, ActionType, WorkflowId, WorkflowName, Created.
    /// </summary>
    public record UsersAuditorActionsDto
    {
        public int CardId { get; init; }
        public string CardName { get; init; } = string.Empty;
        public string ActionType { get; init; } = string.Empty;
        public int WorkflowId { get; init; }
        public string WorkflowName { get; init; } = string.Empty;
        public DateTime Created { get; init; }
    }
}
