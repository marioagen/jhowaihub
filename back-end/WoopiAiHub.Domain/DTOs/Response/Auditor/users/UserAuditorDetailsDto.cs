namespace WoopiAiHub.Domain.DTOs.Response.Auditor
{
    /// <summary>
    /// Full audit details for one user: UserId, UserName, Teams, Profiles, log counts (total and by action type), and list of actions.
    /// </summary>
    public record UserAuditorDetailsDto
    {
        public Guid UserId { get; init; }
        public string UserName { get; init; } = string.Empty;
        public ICollection<AuditorTeamItemDto> Teams { get; init; } = new List<AuditorTeamItemDto>();
        public ICollection<AuditorProfileItemDto> Profiles { get; init; } = new List<AuditorProfileItemDto>();
        public int LogCountTotal { get; init; }
        public ICollection<UserAuditorActionTypeCountDto> LogCountByActionType { get; init; } = new List<UserAuditorActionTypeCountDto>();
        public ICollection<UserAuditorActionDto> Actions { get; init; } = new List<UserAuditorActionDto>();
    }
}
