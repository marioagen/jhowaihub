using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using WoopiAiHub.Domain.DTOs.Messaging;
using WoopiAiHub.Domain.Interfaces.Messaging;
using WoopiAiHub.Domain.Interfaces.Services;
using WoopiAiHub.Infrastructure.Messaging.Configuration;
using WoopiAiHub.Infrastructure.Messaging.Consumers;

namespace WoopiAiHub.Application.Messaging
{
    public class SubscriptionConsumer : BaseConsumer
    {
        private readonly IMessageConsumer<TenantSubscriptionDto> _consumer;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<SubscriptionConsumer> _logger;
        private readonly MessageQueues _queues;

        public SubscriptionConsumer(IServiceScopeFactory scopeFactory,
                           IConfiguration configuration,
                           IMessageConsumer<TenantSubscriptionDto> consumer,
                           ILogger<SubscriptionConsumer> logger,
                           IOptions<MessageQueues> queues) : base(configuration)
        {
            _scopeFactory = scopeFactory;
            _queues = queues.Value;
            _consumer = consumer;
            _logger = logger;
        }

        /// <summary>
        /// Execute the background service to consume messages from the marketplace activation queue.
        /// </summary>
        /// <param name="stoppingToken"></param>
        /// <returns></returns>
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await _consumer.ConsumerAsync(_queues.MarketplaceSubscriptionQueue, async message =>
            {
                using var scope = _scopeFactory.CreateScope();
                try
                {   
                    var tenantServices = scope.ServiceProvider.GetRequiredService<ITenantServices>();
                    tenantServices.ProcessSubscription(message);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing marketplace activation message for Tenant: {TenantName}", message.Name);
                }
            });
        }
    }
}
