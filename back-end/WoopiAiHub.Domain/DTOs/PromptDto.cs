namespace WoopiAiHub.Domain.DTOs
{
    public record PromptDto : PromptBaseDto
    {
        public string Text { get; set; } = string.Empty;
        public Guid IdUser { get; set; } = Guid.Empty;
        public bool IsOwner { get; set; }
        public DateTime Created { get; set; }
        public bool IsEdited { get; set; }
        public bool IsImported { get; set; }
        public bool InternalPrompt { get; set; } = false;
    }
}
