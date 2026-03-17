namespace WoopiAiHub.Domain.DTOs
{
    public record PromptUpdateDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Text { get; set; } = string.Empty;
        public bool EnableAccessToMcp { get; private set; } = false;
        public List<int> ApiTemplatesSelected { get; set; } = new ();
    }
}
