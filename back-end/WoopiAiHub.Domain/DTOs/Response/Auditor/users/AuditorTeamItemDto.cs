namespace WoopiAiHub.Domain.DTOs.Response.Auditor
{
    /// <summary>
    /// Team id and name for user audit summary list.
    /// </summary>
    public record AuditorTeamItemDto
    {
        public int TeamId { get; init; }
        public string TeamName { get; init; } = string.Empty;
    }
}
