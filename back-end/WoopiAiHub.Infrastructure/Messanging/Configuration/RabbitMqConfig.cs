namespace WoopiAiHub.Infrastructure.Messanging.Configuration
{
    public class RabbitMqConfig : MessageConfig
    {
        public string VirtualHost { get; set; } = "/";
    }
}
