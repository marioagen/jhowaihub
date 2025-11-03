using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using WoopiAiHub.Domain.Interfaces.Messaging;
using WoopiAiHub.Infrastructure.Messaging.Configuration;

namespace WoopiAiHub.Infrastructure.Messaging.Managers
{
    public class RabbitMqManager : IMessageManager
    {
        private readonly RabbitMqConfig _config;
        private readonly MessageQueues _queues;
        public IConnectionFactory ConnectionFactory { get; set; }

        public RabbitMqManager(IOptions<RabbitMqConfig> config,
                               IOptions<MessageQueues> queues)
        {
            _config = config.Value;
            _queues = queues.Value;
            ConnectionFactory = CreateConnectionFactory();
        }

        /// <summary>
        /// Initialize RabbitMQ connection and create queues with Dead Letter Queue (DLQ) support.
        /// For each main queue, creates:
        /// - Dead Letter Exchange (DLX)
        /// - Dead Letter Queue (DLQ)
        /// - Main queue configured to send failed messages to DLQ
        /// </summary>
        /// <returns></returns>
        public async Task CreateQueuesAsync()
        {
            using var connection = await this.ConnectionFactory.CreateConnectionAsync();
            using var channel = await connection.CreateChannelAsync();

            // Define Dead Letter Exchange name
            const string dlxName = "dlx.exchange";

            // Declare Dead Letter Exchange (DLX) once
            // This exchange will receive all messages that failed after retries
            await channel.ExchangeDeclareAsync(
                exchange: dlxName,
                type: "direct",
                durable: true,
                autoDelete: false,
                arguments: null
            );

            foreach (var queue in _queues.Queues())
            {
                // Define DLQ name for this queue
                var dlqName = $"{queue}.dlq";

                // Create the Dead Letter Queue (DLQ)
                // This queue will store messages that failed after all retry attempts
                await channel.QueueDeclareAsync(
                    queue: dlqName,
                    durable: true,
                    exclusive: false,
                    autoDelete: false,
                    arguments: null
                );

                // Bind DLQ to the Dead Letter Exchange
                // Messages sent to DLX with this routing key will go to this DLQ
                await channel.QueueBindAsync(
                    queue: dlqName,
                    exchange: dlxName,
                    routingKey: queue, // Use original queue name as routing key
                    arguments: null
                );

                // Configure main queue with DLQ settings
                // When a message is rejected (BasicNack with requeue=false), it goes to DLX
                var queueArguments = new Dictionary<string, object?>
                {
                    { "x-dead-letter-exchange", dlxName },      // Send failed messages to DLX
                    { "x-dead-letter-routing-key", queue }      // Use queue name as routing key in DLX
                };

                // Create the main queue with DLQ configuration
                await channel.QueueDeclareAsync(
                    queue: queue,
                    durable: true,
                    exclusive: false,
                    autoDelete: false,
                    arguments: queueArguments
                );
            }
        }

        /// <summary>
        /// Method to create a channel
        /// </summary>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public async Task<T> CreateChannel<T>()
        {
            var connection = await this.ConnectionFactory.CreateConnectionAsync();

            return (T) await connection.CreateChannelAsync();
        }

        /// <summary>
        /// Create connection factory for rabbitMQ
        /// </summary>
        /// <returns></returns>
        private ConnectionFactory CreateConnectionFactory()
        {
            var factory = new ConnectionFactory
            {
                HostName = _config.HostName,
                UserName = _config.UserName,
                Password = _config.Password,
                VirtualHost = _config.VirtualHost,
                ClientProvidedName = "WoopiAiHub"
            };

            return factory;
        }
    }
}
