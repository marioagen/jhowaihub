namespace WoopiAiHub.Domain.DTOs.Request
{
    public class TeamUpdateDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public List<Guid> UserIds { get; set; } = new List<Guid>();
        public List<int> ProfileIds { get; set; } = new List<int>();
    }
}
