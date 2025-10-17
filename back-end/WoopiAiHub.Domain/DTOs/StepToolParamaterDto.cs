namespace WoopiAiHub.Domain.DTOs
{
    public class StepToolParameterDto
    {
        public int Id { get; set; }
        public string Type { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
        public bool RequiredFile { get; set; }
        public Guid? WebhookId { get; set; }
    }
}
