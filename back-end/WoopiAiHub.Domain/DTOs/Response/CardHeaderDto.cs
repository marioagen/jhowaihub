namespace WoopiAiHub.Domain.DTOs.Response
{
    public record class CardHeaderDto
    {
        public string CardName { get; set; } = string.Empty;
        public string WorkflowName { get; set; } = string.Empty;
        public int WorkflowId { get; set; }
        public string StatusName { get; set; } = string.Empty;
        public int CurrentStepOrder { get; set; }
        public int? DocumentBatchId { get; set; }
    }
}
