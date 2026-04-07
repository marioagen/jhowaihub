namespace WoopiAiHub.Domain.DTOs
{
    public record PromptDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Text { get; set; } = string.Empty;
        public Guid IdUser { get; set; } = Guid.Empty;
        public bool IsOwner { get; set; }
        public DateTime Created { get; set; }
        public bool IsEdited { get; set; }
        public bool IsImported { get; set; }
        public bool EnableAccessToMcp { get; set; } = false;        
        public List<PromptApiTemplateDto> PromptApiTemplates { get; set; } = [];
        public string OwnerName { get; set; } = string.Empty;
        public string OwnerEmail { get; set; } = string.Empty;
    }
}
