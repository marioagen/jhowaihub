namespace WoopiAiHub.Domain.DTOs.Request
{
    public record struct UpdateCardStepStatusDto
    {
        public int CardId { get; set; }
        public int NextStepOrder { get; set; }
        public int WorkflowId { get; set; }
    }
}
