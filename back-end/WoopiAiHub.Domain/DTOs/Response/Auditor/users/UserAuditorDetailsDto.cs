namespace WoopiAiHub.Domain.DTOs.Response.Auditor
{
    public record UserAuditorDetailsDto
    {
        public Guid UserId { get; init; }
        public string UserName { get; init; } = string.Empty;
        public ICollection<UsersAuditorTeamsDto> Teams { get; init; } = new List<UsersAuditorTeamsDto>();
        public ICollection<UsersAuditorProfilesDto> Profiles { get; init; } = new List<UsersAuditorProfilesDto>();
        public int LogCountTotal { get; init; }
        public ICollection<UsersAuditorActionTypeCountsDto> LogCountByActionType { get; init; } = new List<UsersAuditorActionTypeCountsDto>();
        public ICollection<UsersAuditorActionsDto> Actions { get; init; } = new List<UsersAuditorActionsDto>();
    }
}
