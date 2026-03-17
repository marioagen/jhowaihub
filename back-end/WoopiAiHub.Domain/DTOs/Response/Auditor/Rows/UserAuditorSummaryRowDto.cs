using WoopiAiHub.Domain.DTOs.Response.Auditor;

namespace WoopiAiHub.Domain.DTOs.Response.Auditor.Rows
{
    /// <summary>
    /// Raw audit row for user summary. Used by repository and service to build UserAuditorSummaryDto.
    /// </summary>
    public record UserAuditorSummaryRowDto
    {
        public Guid UserId { get; init; }
        public string UserName { get; init; } = string.Empty;
        public int WorkflowId { get; init; }
        public IEnumerable<UsersAuditorTeamsDto>? Teams { get; init; }
        public int? ProfileId { get; init; }
        public string ProfileName { get; init; } = string.Empty;
    }
}
