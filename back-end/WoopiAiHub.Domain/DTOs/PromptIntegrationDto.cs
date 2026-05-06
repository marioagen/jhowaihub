namespace WoopiAiHub.Domain.DTOs
{
    public record PromptIntegrationDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Text { get; set; } = string.Empty;
    }
}
