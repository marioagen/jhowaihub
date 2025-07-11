namespace WoopiAiHub.Domain.DTOs.Request
{
    public class TeamCreateDto
    {
        public string Name { get; set; } = string.Empty;
        public List<string> UserIds { get; set; } = new List<string>();
    }
}
