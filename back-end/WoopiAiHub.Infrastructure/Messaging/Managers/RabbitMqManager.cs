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
        /// Initialize RabbitMQ connection and create queues 
        /// </summary>
        /// <returns></returns>
        public async Task CreateQueuesAsync()
        {
            using var connection = await this.ConnectionFactory.CreateConnectionAsync();
            using var channel = await connection.CreateChannelAsync();

            foreach (var queue in _queues.Queues())
            {
                await channel.QueueDeclareAsync(queue: queue, durable: true, exclusive: false, autoDelete: false, arguments: null);
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
