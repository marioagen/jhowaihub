using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using WoopiAiHub.Domain.DTOs;
using WoopiAiHub.Domain.Interfaces.Messaging;
using WoopiAiHub.Domain.Interfaces.Services;
using WoopiAiHub.Infrastructure.Messaging.Configuration;
using WoopiAiHub.Infrastructure.Messaging.Consumers;

namespace WoopiAiHub.Application.Messaging
{
    public class SubscriptionEndPeriodConsumer : BaseConsumer
    {
        private readonly IMessageConsumer<SubscriptionPeriodDto> _consumer;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<SubscriptionEndPeriodConsumer> _logger;
        private readonly MessageQueues _queues;

        public SubscriptionEndPeriodConsumer(IServiceScopeFactory scopeFactory,
                           IConfiguration configuration,
                           IMessageConsumer<SubscriptionPeriodDto> consumer,
                           ILogger<SubscriptionEndPeriodConsumer> logger,
                           IOptions<MessageQueues> queues) : base(configuration)
        {
            _scopeFactory = scopeFactory;
            _queues = queues.Value;
            _consumer = consumer;
            _logger = logger;
        }

        /// <summary>
        /// Executes the background service operation to process messages from the marketplace end subscription period
        /// queue.
        /// </summary>
        /// <remarks>This method is called by the host to begin processing. It listens for messages on the
        /// marketplace end subscription period queue and processes each message asynchronously until the <paramref
        /// name="stoppingToken"/> is signaled.</remarks>
        /// <param name="stoppingToken">A <see cref="CancellationToken"/> that is triggered when the host is performing a graceful shutdown.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous execution of the background service.</returns>
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await _consumer.ConsumerAsync(_queues.MarketplaceEndSubscriptionPeriodQueue, async message =>
            {
                using var scope = _scopeFactory.CreateScope();
                try
                {
                    var connectionString = await GetConnectionStringAsync(scope, message.Tenant!);
                    var httpAccessor = scope.ServiceProvider.GetRequiredService<IHttpContextAccessor>();
                    httpAccessor.HttpContext ??= new DefaultHttpContext();
                    httpAccessor.HttpContext.Items["TenantConnection"] = connectionString;

                    var subscriptionPeriodServices = scope.ServiceProvider.GetRequiredService<ISubscriptionPeriodServices>();
                    await subscriptionPeriodServices.CreateAsync(message.PeriodStart, message.PeriodEnd, false);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing marketplace end subscription period message.");
                }
            });
        }
    }
}
