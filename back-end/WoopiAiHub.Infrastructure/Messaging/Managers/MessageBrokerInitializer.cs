using Microsoft.Extensions.Hosting;
using WoopiAiHub.Domain.Interfaces.Messaging;

namespace WoopiAiHub.Infrastructure.Messaging.Managers
{
    /// <summary>
    /// MessageBrokerInitializer is responsible for initializing the message broker. Since the dependency is injected, it starts together with the application.
    /// </summary>
    public class MessageBrokerInitializer : IHostedService
    {
        private readonly IMessageManager _messageManager;

        public MessageBrokerInitializer(IMessageManager messageManager)
        {
            _messageManager = messageManager;
        }

        /// <summary>
        /// On application startup, the broker is initialized according to the configuration.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await _messageManager.CreateQueuesAsync().ConfigureAwait(false);
        }

        /// <summary>
        /// When application shutdown
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
