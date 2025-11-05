using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using WoopiAiHub.Domain.Interfaces.Messaging;
using WoopiAiHub.Infrastructure.Messaging.Configuration;
using WoopiAiHub.Infrastructure.Messaging.Consumers;
using WoopiAiHub.Infrastructure.Messaging.Managers;
using WoopiAiHub.Infrastructure.Messaging.Publishers;
using WoopiAiHub.Infrastructure.Multitenancy;

namespace WoopiAiHub.Infrastructure.DependencyInjection
{
    public static class Extension
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services,
                                                           IConfiguration configuration)
        {
            services.AddScoped<ITenantContextService, TenantContextService>();
            services.AddHttpContextAccessor();
            
            var messageBroker = configuration["Messaging:BrokerType"];
            switch (messageBroker)
            {
                case "RabbitMQ":
                    services.Configure<RabbitMqConfig>(configuration.GetSection("Messaging:Brokers:RabbitMQ"));
                    services.PostConfigure<RabbitMqConfig>(options =>
                    {
                        // Ensure default values if not set in configuration
                        if (options.MaxRetryAttempts <= 0)
                            options.MaxRetryAttempts = 3;
                        if (options.InitialRetryDelaySeconds <= 0)
                            options.InitialRetryDelaySeconds = 2;
                    });
                    services.AddSingleton<RabbitMqManager>();
                    services.AddSingleton<IMessageManager, RabbitMqManager>();
                    services.AddSingleton(typeof(IMessagePublisher<>), typeof(RabbitMqPublisher<>));
                    services.AddSingleton(typeof(IMessageConsumer<>), typeof(RabbitMqConsumer<>));
                    break;
                default:
                    throw new InvalidOperationException("Messaging broker is not configured correctly.");
            }
            services.AddSingleton<IHostedService, MessageBrokerInitializer>();
            services.Configure<MessageQueues>(configuration.GetSection("Messaging:Queues"));

            return services;
        }
    }
}
