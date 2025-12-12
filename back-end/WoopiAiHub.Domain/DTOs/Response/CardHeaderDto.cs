namespace WoopiAiHub.Domain.DTOs.Response
{
    public record class CardHeaderDto
    {
        public string CardName { get; set; } = string.Empty;
        public string WorkflowName { get; set; } = string.Empty;
    }
}
