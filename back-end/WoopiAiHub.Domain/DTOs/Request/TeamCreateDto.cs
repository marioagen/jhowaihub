namespace WoopiAiHub.Domain.DTOs.Request
{
    public class TeamCreateDto
    {
        public string Name { get; set; } = string.Empty;
        public List<Guid> UserIds { get; set; } = new List<Guid>();
    }
}
