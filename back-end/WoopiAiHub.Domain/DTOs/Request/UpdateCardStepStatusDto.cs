namespace WoopiAiHub.Domain.DTOs.Request
{
    public record struct UpdateCardStepStatusDto
    {
        public int CardId { get; set; }
        public int StepId { get; set; }
        public int StatusId { get; set; }
    }
}
