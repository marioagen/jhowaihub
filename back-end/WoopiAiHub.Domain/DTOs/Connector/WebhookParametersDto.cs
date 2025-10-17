namespace WoopiAiHub.Domain.DTOs.Connector
{
    public record class WebhookParametersDto
    {
        public string HttpMethod { get; set; } = string.Empty;
    }
}
