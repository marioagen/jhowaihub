namespace WoopiAiHub.Domain.DTOs.Response
{
    public record class PromptTemplatesResponse
    {
        public List<PromptTemplateDto> Prompts { get; set; } = [];
    }
}
