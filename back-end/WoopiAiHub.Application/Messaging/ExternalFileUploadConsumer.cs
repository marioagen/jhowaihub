using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using WoopiAiHub.Domain.DTOs.Messaging;
using WoopiAiHub.Domain.Interfaces.Messaging;
using WoopiAiHub.Domain.Interfaces.Services.Automation;
using WoopiAiHub.Infrastructure.Messaging.Configuration;
using WoopiAiHub.Infrastructure.Messaging.Consumers;

namespace WoopiAiHub.Application.Messaging
{
    public class ExternalFileUploadConsumer : BaseConsumer
    {
        private readonly IMessageConsumer<ExternalFileUploadDto> _consumer;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<ExternalFileUploadConsumer> _logger;
        private readonly MessageQueues _queues;

        public ExternalFileUploadConsumer(IServiceScopeFactory scopeFactory,
                              IConfiguration configuration,
                              IMessageConsumer<ExternalFileUploadDto> consumer,
                              ILogger<ExternalFileUploadConsumer> logger,
                              IOptions<MessageQueues> queues) : base(configuration)
        {
            _consumer = consumer;
            _scopeFactory = scopeFactory;
            _logger = logger;
            _queues = queues.Value;
        }

        /// <summary>
        /// Executes the background processing logic for handling external file upload messages.
        /// </summary>
        /// <remarks>This method listens for messages from the external file upload queue and processes
        /// each message asynchronously. It is typically invoked by the hosting infrastructure as part of a background
        /// service lifecycle.</remarks>
        /// <param name="stoppingToken">A <see cref="CancellationToken"/> that signals when the operation should be canceled.</param>
        /// <returns></returns>
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await _consumer.ConsumerAsync(_queues.ExternalFileUploadQueue, async message =>
            {
                using var scope = _scopeFactory.CreateScope();
                try
                {
                    var connectionString = await GetConnectionStringAsync(scope, message.Tenant!);
                    var httpAccessor = scope.ServiceProvider.GetRequiredService<IHttpContextAccessor>();
                    httpAccessor.HttpContext ??= new DefaultHttpContext();
                    httpAccessor.HttpContext.Items["TenantConnection"] = connectionString;
                    var externalFileUploadServices = scope.ServiceProvider.GetRequiredService<IExternalFileUploadServices>();
                    await externalFileUploadServices.ProcessExternalFileUpload(message);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to process the answer response.");
                }
            });
        }
    }
}
