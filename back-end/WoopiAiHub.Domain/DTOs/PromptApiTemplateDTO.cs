using WoopiAiHub.Domain.DTOs.Response;

namespace WoopiAiHub.Domain.DTOs
{
    public class PromptApiTemplateDTO
    {
        public int Id { get; set; }
        
        public int PromptId { get; set; }
        public PromptDto Prompt { get; set; }
        public int ApiTemplateId { get; set; }
        public ApiTemplateDto ApiTemplate { get; set; }
    }
}