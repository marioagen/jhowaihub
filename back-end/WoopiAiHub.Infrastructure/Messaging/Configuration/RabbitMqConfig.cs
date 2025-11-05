namespace WoopiAiHub.Infrastructure.Messaging.Configuration
{
    public class RabbitMqConfig : MessageConfig
    {
        public string VirtualHost { get; set; } = "/";
        
        /// <summary>
        /// Maximum number of retry attempts for message processing before sending to DLQ.
        /// Default is 3 (set via PostConfigure if not specified in appsettings).
        /// </summary>
        public int MaxRetryAttempts { get; set; }
        
        /// <summary>
        /// Initial retry delay in seconds for exponential backoff.
        /// Default is 2 seconds (set via PostConfigure if not specified in appsettings).
        /// </summary>
        public int InitialRetryDelaySeconds { get; set; }
    }
}
