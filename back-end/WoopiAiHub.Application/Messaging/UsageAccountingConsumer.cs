using Microsoft.AspNetCore.Http;
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
    public class UsageAccountingConsumer : BaseConsumer
    {
        private readonly IMessageConsumer<UsageAccountingDto> _consumer;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<UsageAccountingConsumer> _logger;
        private readonly MessageQueues _queues;

        public UsageAccountingConsumer(IServiceScopeFactory scopeFactory,
                                       IConfiguration configuration,
                                       IMessageConsumer<UsageAccountingDto> consumer,
                                       ILogger<UsageAccountingConsumer> logger,
                                       IOptions<MessageQueues> queues) : base(configuration)
        {
            _scopeFactory = scopeFactory;
            _queues = queues.Value;
            _consumer = consumer;
            _logger = logger;
        }

        /// <summary>
        /// Executes the background service that consumes usage accounting messages
        /// and persists them in UsageDaily for the tenant identified in the payload.
        /// </summary>
        /// <param name="stoppingToken">Token used to signal cancellation of the hosted service.</param>
        /// <returns></returns>
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await _consumer.ConsumerAsync(_queues.UsageAccountingQueue, async message =>
            {
                using var scope = _scopeFactory.CreateScope();
                try
                {
                    await ProcessMessageAsync(scope, message);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex,
                        "Failed to process usage accounting message for tenant {Tenant} and email {Email}.",
                        message.Tenant,
                        message.Email);
                }
            });
        }

        /// <summary>
        /// Configures the tenant connection in the current scope and forwards
        /// the usage data to <see cref="IUsageDailyServices"/>.
        /// </summary>
        /// <param name="scope">DI scope created for this message.</param>
        /// <param name="message">Usage accounting payload received from the queue.</param>
        /// <returns></returns>
        private async Task ProcessMessageAsync(IServiceScope scope, UsageAccountingDto message)
        {
            var connectionString = await GetConnectionStringAsync(scope, message.Tenant);
            var httpAccessor = scope.ServiceProvider.GetRequiredService<IHttpContextAccessor>();
            httpAccessor.HttpContext ??= new DefaultHttpContext();
            httpAccessor.HttpContext.Items["TenantConnection"] = connectionString;

            var usageDailyServices = scope.ServiceProvider.GetRequiredService<IUsageDailyServices>();

            await usageDailyServices.AddByValuesAsync(
                message.UsageTypeName,
                message.Email,
                message.Count,
                message.ModelEmbeddingName ?? string.Empty,
                message.WorkflowId,
                message.Origin);
        }
    }
}
