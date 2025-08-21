using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using WoopiAiHub.Domain.Interfaces.Messenging;
using WoopiAiHub.Infrastructure.Messanging.Configuration;
using WoopiAiHub.Infrastructure.Messanging.Consumers;
using WoopiAiHub.Infrastructure.Messanging.Managers;
using WoopiAiHub.Infrastructure.Messanging.Publishers;
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
