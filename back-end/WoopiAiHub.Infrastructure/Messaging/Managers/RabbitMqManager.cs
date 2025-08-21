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
        private IConnection? _connection;

        public RabbitMqManager(IOptions<RabbitMqConfig> config,
                               IOptions<MessageQueues> queues)
        {
            _config = config.Value;
            _queues = queues.Value;
        }

        /// <summary>
        /// Initialize RabbitMQ connection and create queues 
        /// </summary>
        /// <returns></returns>
        public async Task CreateQueuesAsync()
        {
            var factory = new ConnectionFactory
            {
                HostName = _config.HostName,
                UserName = _config.UserName,
                Password = _config.Password,
                VirtualHost = _config.VirtualHost
            };

            _connection = await factory.CreateConnectionAsync();
            using var channel = await _connection.CreateChannelAsync();

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
            if (_connection == null || !_connection.IsOpen)
                throw new InvalidOperationException("RabbitMQ connection is not open.");

            return (T)await _connection.CreateChannelAsync();
        }
    }
}
