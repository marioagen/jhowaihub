using System;

namespace WoopiAiHub.Domain.DTOs.Request
{
    public record ImportedPromptDto
    {
        public Guid TemplateId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Text { get; set; } = string.Empty;
        public DateTime Created { get; set; }
    }
}
