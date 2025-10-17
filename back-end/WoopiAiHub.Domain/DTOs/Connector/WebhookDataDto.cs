namespace WoopiAiHub.Domain.DTOs.Connector
{
    public record class WebhookDataDto
    {
        public List<WebhookDto> Data { get; set; } = [];
    }
}
