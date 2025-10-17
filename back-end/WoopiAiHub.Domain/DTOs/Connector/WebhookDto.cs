namespace WoopiAiHub.Domain.DTOs.Connector
{
    public record class WebhookDto
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public List<WebhookNodeDto> Nodes { get; set; } = [];
    }
}
