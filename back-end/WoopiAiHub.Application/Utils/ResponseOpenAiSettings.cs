namespace WoopiAiHub.Application.Utils
{
    public class ResponseOpenAiSettings
    {
        public double Temperature { get; set; }
        public string Model { get; set; } = string.Empty;
        public string ApiVersion { get; set; } = string.Empty;
        public string McpAddress { get; set; } = string.Empty;
        public string SessionIdKey { get; set; } = string.Empty;
    }
}