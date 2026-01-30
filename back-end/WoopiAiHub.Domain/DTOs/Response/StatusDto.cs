namespace WoopiAiHub.Domain.DTOs.Response
{
    public record struct StatusDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string? Label { get; set; }
        public string Color { get; set; }
    }
}
