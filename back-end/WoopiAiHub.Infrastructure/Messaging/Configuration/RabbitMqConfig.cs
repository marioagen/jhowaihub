namespace WoopiAiHub.Infrastructure.Messaging.Configuration
{
    public class RabbitMqConfig : MessageConfig
    {
        public string VirtualHost { get; set; } = "/";
        public int MaxRetryAttempts { get; set; } = 3;
        public int InitialRetryDelaySeconds { get; set; } = 3;
    }
}
