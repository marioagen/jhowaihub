namespace WoopiAiHub.Domain.DTOs.Response.Auditor
{
    public record UsersAuditorProfilesDto
    {
        public int ProfileId { get; init; }
        public string ProfileName { get; init; } = string.Empty;
    }
}
