namespace WoopiAiHub.Domain.DTOs.Response.Auditor
{
    /// <summary>
    /// Profile id and name for user audit summary list.
    /// </summary>
    public record AuditorProfileItemDto
    {
        public int ProfileId { get; init; }
        public string ProfileName { get; init; } = string.Empty;
    }
}
