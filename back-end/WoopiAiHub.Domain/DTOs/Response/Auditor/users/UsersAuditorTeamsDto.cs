namespace WoopiAiHub.Domain.DTOs.Response.Auditor
{
    public record UsersAuditorTeamsDto
    {
        public int TeamId { get; init; }
        public string TeamName { get; init; } = string.Empty;
    }
}
