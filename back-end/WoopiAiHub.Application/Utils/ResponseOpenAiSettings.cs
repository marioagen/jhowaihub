namespace WoopiAiHub.Application.Utils
{
    public class ResponseOpenAiSettings
    {
        public double Temperature { get; set; }
        public string Model { get; set; } = string.Empty;
        public string ApiVersion { get; set; } = string.Empty;
        public string McpAddress { get; set; } = string.Empty;
        public int MaxToolCalls { get; set; } = 20;
        public string SessionIdKey { get; set; } = string.Empty;
        public string JWTKey { get; set; } = string.Empty;
        public string JWTIssuer { get; set; } = string.Empty;
        public string JWTAudience { get; set; } = string.Empty;
        public string JWTUser { get; set; } = string.Empty;
        public int JWTExpirationTime { get; set; } = 20;
        public string Instructions { get; set; } = string.Empty;
    }
}