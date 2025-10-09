namespace WoopiAiHub.Domain.DTOs.Connector
{
    public record class ConnectorDto
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string WebhookId { get; set; } = string.Empty;
    }
}
