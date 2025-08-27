using Newtonsoft.Json;
using System.Text;
using RabbitMQ.Client;
using WoopiAiHub.Domain.Interfaces.Messaging;

namespace WoopiAiHub.Infrastructure.Messaging.Publishers
{
    public class RabbitMqPublisher<T> : IMessagePublisher<T>
    {
        private readonly IMessageManager _manager;

        public RabbitMqPublisher(IMessageManager manager)
        {
            _manager = manager;
        }

        /// <summary>
        /// Publish message in a queue
        /// </summary>
        /// <param name="message"></param>
        /// <param name="queueName"></param>
        /// <returns></returns>
        public async Task PublishAsync(string destination, T message)
        {
            using var channel = await _manager.CreateChannel<IChannel>();

            var messageJson = JsonConvert.SerializeObject(message);
            var body = Encoding.UTF8.GetBytes(messageJson);

            await channel.BasicPublishAsync(exchange: "", routingKey: destination, body: body);
        }
    }
}
