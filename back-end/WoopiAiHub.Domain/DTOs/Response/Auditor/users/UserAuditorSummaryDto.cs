namespace WoopiAiHub.Domain.DTOs.Response.Auditor
{
    public record UserAuditorSummaryDto
    {
        public Guid UserId { get; init; }
        public string UserName { get; init; } = string.Empty;
        public ICollection<UsersAuditorTeamsDto> Teams { get; init; } = new List<UsersAuditorTeamsDto>();
        public ICollection<UsersAuditorProfilesDto> Profiles { get; init; } = new List<UsersAuditorProfilesDto>();
        public int WorkflowCount { get; init; }
        public int LogCount { get; init; }
    }
}
