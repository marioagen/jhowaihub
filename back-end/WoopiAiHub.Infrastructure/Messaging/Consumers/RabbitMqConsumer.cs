using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using WoopiAiHub.Domain.Interfaces.Messaging;
using WoopiAiHub.Infrastructure.Messaging.Managers;

namespace WoopiAiHub.Infrastructure.Messaging.Consumers
{
    public class RabbitMqConsumer<T> : IMessageConsumer<T>
    {
        private readonly IMessageManager _manager;

        public RabbitMqConsumer(IMessageManager manager)
        {
            _manager = manager;
        }

        /// <summary>
        /// Starts consuming messages from the specified queue and processes them using the given function.
        /// </summary>
        /// <param name="destination"></param>
        /// <param name="process"></param>
        /// <returns></returns>
        public async Task ConsumerAsync(string destination, Func<T, Task> process)
        {
            var channel = await _manager.CreateChannel<IChannel>();
            var consumer = new AsyncEventingBasicConsumer(channel);

            consumer.ReceivedAsync += (sender, args) =>
                    HandleMessageAsync(channel, args, process);

            await channel.BasicConsumeAsync(queue: destination, autoAck: false, consumer: consumer)
                         .ConfigureAwait(false);
        }

        /// <summary>
        /// Handles a received message by deserializing it and executing the provided process function.
        /// Acknowledges the message after successful processing.
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="args"></param>
        /// <param name="process"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        private static async Task HandleMessageAsync(IChannel channel, BasicDeliverEventArgs args, Func<T, Task> process)
        {
            var body = args.Body.ToArray();
            var json = Encoding.UTF8.GetString(body);
            var message = JsonConvert.DeserializeObject<T>(json);

            if (message is null)
                throw new InvalidOperationException("The message could not be deserialized.");

            await process(message).ConfigureAwait(false);
            await channel.BasicAckAsync(args.DeliveryTag, multiple: false).ConfigureAwait(false);
        }
    }
}
