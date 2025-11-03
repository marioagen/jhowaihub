using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Polly;
using Polly.Retry;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using WoopiAiHub.Domain.Interfaces.Messaging;
using WoopiAiHub.Infrastructure.Messaging.Managers;

namespace WoopiAiHub.Infrastructure.Messaging.Consumers
{
    public class RabbitMqConsumer<T> : IMessageConsumer<T>
    {
        private const int MaxRetryAttempts = 3;
        private const int InitialRetryDelaySeconds = 2;

        private readonly RabbitMqManager _manager;
        private readonly ILogger<RabbitMqConsumer<T>> _logger;
        private readonly ResiliencePipeline _retryPipeline;

        public RabbitMqConsumer(RabbitMqManager manager, ILogger<RabbitMqConsumer<T>> logger)
        {
            _manager = manager;
            _logger = logger;
            
            // Create Polly retry pipeline once for reuse across all messages
            // This improves performance under high message volume
            _retryPipeline = new ResiliencePipelineBuilder()
                .AddRetry(new RetryStrategyOptions
                {
                    MaxRetryAttempts = MaxRetryAttempts,
                    Delay = TimeSpan.FromSeconds(InitialRetryDelaySeconds),
                    BackoffType = DelayBackoffType.Exponential,
                    UseJitter = true,
                    OnRetry = args =>
                    {
                        // Log retry attempts
                        _logger.LogWarning(
                            "Retry attempt {AttemptNumber} of {MaxAttempts} for message processing. Exception: {Exception}",
                            args.AttemptNumber,
                            MaxRetryAttempts,
                            args.Outcome.Exception?.Message
                        );
                        return ValueTask.CompletedTask;
                    }
                })
                .Build();
        }

        /// <summary>
        /// Starts consuming messages from the specified queue and processes them using the given function.
        /// </summary>
        /// <param name="destination"></param>
        /// <param name="process"></param>
        /// <returns></returns>
        public async Task ConsumerAsync(string destination, Func<T, Task> process)
        {
            var factory = _manager.ConnectionFactory;
            var connection = await factory.CreateConnectionAsync().ConfigureAwait(false);
            var channel = await connection.CreateChannelAsync().ConfigureAwait(false);

            var consumer = new AsyncEventingBasicConsumer(channel);

            consumer.ReceivedAsync += (sender, args) =>
                    HandleMessageAsync(channel, args, process);

            await channel.BasicConsumeAsync(queue: destination, autoAck: false, consumer: consumer)
                         .ConfigureAwait(false);
        }

        /// <summary>
        /// Handles a received message by deserializing it and executing the provided process function.
        /// Uses Polly retry policy with 3 attempts and exponential backoff.
        /// Flow:
        /// 1. Deserialize message
        /// 2. Try to process with Polly retry (3 attempts with exponential backoff)
        /// 3. On success: BasicAck (acknowledge message)
        /// 4. On final failure after retries: BasicNack with requeue=false (send to DLQ)
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="args"></param>
        /// <param name="process"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        private async Task HandleMessageAsync(IChannel channel, BasicDeliverEventArgs args, Func<T, Task> process)
        {
            try
            {
                // Deserialize message
                var body = args.Body.ToArray();
                var json = Encoding.UTF8.GetString(body);
                var message = JsonConvert.DeserializeObject<T>(json);

                if (message is null)
                    throw new InvalidOperationException("The message could not be deserialized.");

                // Execute message processing with retry policy
                // Retry delays: ~2s, ~4s, ~8s (with jitter)
                await _retryPipeline.ExecuteAsync(async ct =>
                {
                    await process(message).ConfigureAwait(false);
                }).ConfigureAwait(false);

                // Success: Acknowledge the message
                await channel.BasicAckAsync(args.DeliveryTag, multiple: false).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                // Final failure after all retry attempts
                // Send message to DLQ by rejecting with requeue=false
                _logger.LogError(ex, "Message processing failed after all retry attempts. Sending to DLQ.");
                await channel.BasicNackAsync(args.DeliveryTag, multiple: false, requeue: false).ConfigureAwait(false);
                // Message has been sent to DLQ, do not re-throw to avoid disrupting the consumer pipeline
            }
        }
    }
}
