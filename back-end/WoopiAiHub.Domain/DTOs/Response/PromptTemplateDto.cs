using System;

namespace WoopiAiHub.Domain.DTOs.Response
{
    public record class PromptTemplateDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Text { get; set; } = string.Empty;
        public DateTime Created { get; set; }
    }
}
