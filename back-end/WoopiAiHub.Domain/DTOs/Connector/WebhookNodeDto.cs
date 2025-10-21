namespace WoopiAiHub.Domain.DTOs.Connector
{
    public record class WebhookNodeDto
    {
        public WebhookParametersDto Parameters { get; set; } = new WebhookParametersDto();
        public string WebhookId { get; set; } = string.Empty;
    }
}
