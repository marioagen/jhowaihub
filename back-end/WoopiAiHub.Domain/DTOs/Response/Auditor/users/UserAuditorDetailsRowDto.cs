using WoopiAiHub.Domain.DTOs.Response.Auditor;
using WoopiAiHub.Domain.Enum.Audit;

namespace WoopiAiHub.Domain.DTOs.Response.Auditor.Users
{
    public record UserAuditorDetailsRowDto
    {
        public Guid UserId { get; init; }
        public string UserName { get; init; } = string.Empty;
        public int WorkflowId { get; init; }
        public string WorkflowName { get; init; } = string.Empty;
        public IEnumerable<UsersAuditorTeamsDto>? Teams { get; init; }
        public int? ProfileId { get; init; }
        public string ProfileName { get; init; } = string.Empty;
        public AuditCardActionType ActionType { get; init; }
        public int CardId { get; init; }
        public string CardName { get; init; } = string.Empty;
        public DateTime Created { get; init; }
    }
}
