namespace WoopiAiHub.Domain.DTOs.Response.Auditor
{
    /// <summary>
    /// One user row in the user audit list: UserId, UserName, Teams, Profiles, WorkflowCount, LogCount.
    /// </summary>
    public record UserAuditorSummaryDto
    {
        public Guid UserId { get; init; }
        public string UserName { get; init; } = string.Empty;
        public ICollection<AuditorTeamItemDto> Teams { get; init; } = new List<AuditorTeamItemDto>();
        public ICollection<AuditorProfileItemDto> Profiles { get; init; } = new List<AuditorProfileItemDto>();
        public int WorkflowCount { get; init; }
        public int LogCount { get; init; }
    }
}
