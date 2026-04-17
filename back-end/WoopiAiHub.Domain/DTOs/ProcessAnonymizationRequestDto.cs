using WoopiAiHub.Domain.Enum;

namespace WoopiAiHub.Domain.DTOs
{
    public record ProcessAnonymizationRequestDto
    {
        public int DocumentId { get; set; }
        public int CardId { get; set; }
        public int WorkflowId { get; set; }
        public AnonymizationType? AnonymizationType { get; set; }
        public int? PromptId { get; set; }
    }
}
