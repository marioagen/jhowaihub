using WoopiAiHub.Domain.DTOs.Response;

namespace WoopiAiHub.Domain.DTOs
{
    public record class PromptApiTemplateDto
    {
        public int Id { get; set; }
        public int PromptId { get; set; }
        public PromptDto Prompt { get; set; } = new();
        public int ApiTemplateId { get; set; }
        public ApiTemplateDto ApiTemplate { get; set; } = new();
    }
}