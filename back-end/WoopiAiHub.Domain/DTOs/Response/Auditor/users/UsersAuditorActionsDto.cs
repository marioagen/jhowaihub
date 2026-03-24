namespace WoopiAiHub.Domain.DTOs.Response.Auditor
{
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
