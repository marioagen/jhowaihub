namespace WoopiAiHub.Domain.DTOs.Response
{
    /// <summary>
    /// Simplified DTO for teams containing only Id and Name.
    /// Used for performance optimization in scenarios where full team data is not needed.
    /// </summary>
    public record class TeamSimpleDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}

