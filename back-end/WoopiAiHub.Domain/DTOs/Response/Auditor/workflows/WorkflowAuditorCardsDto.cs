namespace WoopiAiHub.Domain.DTOs.Response.Auditor
{
    public record WorkflowAuditorCardsDto
    {
        public int CardId { get; init; }
        public string CardName { get; init; } = string.Empty;
        public string CardStatus { get; init; } = string.Empty;
        public int StepId { get; init; }
        public string StepName { get; init; } = string.Empty;
        public Guid UserId { get; init; }
        public string UserName { get; init; } = string.Empty;
        public string ActionType { get; init; } = string.Empty;
        public DateTime Created { get; init; }
    }
}
