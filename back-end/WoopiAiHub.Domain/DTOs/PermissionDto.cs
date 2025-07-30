namespace WoopiAiHub.Domain.DTOs
{
    public record class PermissionDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public DateTime Created { get; set; }
    }
}
