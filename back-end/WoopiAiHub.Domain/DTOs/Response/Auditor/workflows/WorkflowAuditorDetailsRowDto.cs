using WoopiAiHub.Domain.Enum.Audit;

namespace WoopiAiHub.Domain.DTOs.Response.Auditor.Workflows
{
    /// <summary>
    /// Raw audit row for workflow details. Used by repository and service to build WorkflowAuditorDetailsDto.
    /// </summary>
    public record WorkflowAuditorDetailsRowDto
    {
        public int Id { get; init; }
        public int CardId { get; init; }
        public int DocumentId { get; init; }
        public int WorkflowId { get; init; }
        public string WorkflowName { get; init; } = string.Empty;
        public DateTime Created { get; init; }
        public Guid UserId { get; init; }
        public string UserName { get; init; } = string.Empty;
        public AuditCardActionType ActionType { get; init; }
        public string CardName { get; init; } = string.Empty;
        public string CardStatus { get; init; } = string.Empty;
        public int StepId { get; init; }
        public string StepName { get; init; } = string.Empty;
    }
}
