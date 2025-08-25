namespace WoopiAiHub.Infrastructure.Messaging.Configuration
{
    public class RabbitMqConfig : MessageConfig
    {
        public string VirtualHost { get; set; } = "/";
    }
}
