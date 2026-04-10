namespace WoopiAiHub.Domain.DTOs.Request
{
    public record PromptTestRequestDto
    {
        public string PromptText { get; set; } = string.Empty;
        public string ContextText { get; set; } = string.Empty;
    }
}
