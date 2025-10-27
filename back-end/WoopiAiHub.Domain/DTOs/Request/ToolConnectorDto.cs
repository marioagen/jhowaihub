namespace WoopiAiHub.Domain.DTOs.Request
{
    public record class ToolConnectorDto
    {
        public string ConnectorUrl { get; set; } = string.Empty;
        public string ConnectorApiKey { get; set; } = string.Empty;
    }
}
