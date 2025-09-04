using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using WoopiAiHub.Domain.Enum;
using WoopiAiHub.Domain.Interfaces.Repository.Cache;

namespace WoopiAiHub.Infrastructure.Messaging.Consumers
{
    public abstract class BaseConsumer : BackgroundService
    {
        private readonly IConfiguration _configuration;

        protected BaseConsumer(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        /// <summary>
        /// Configure the tenant context based on the message and module.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="module"></param>
        /// <param name="tenantSelector"></param>
        /// <returns></returns>
        protected async Task<string> GetConnectionStringAsync(IServiceScope scope,
                                                              string tenantName,
                                                              ColTypeModule module)
        {
            var tenantCacheService = scope.ServiceProvider.GetRequiredService<ITenantCacheServices>();            
            var tenant = await tenantCacheService.FindTenantAsync(tenantName, module);
            var template = _configuration.GetConnectionString("TemplateConnection");
            var connectionString = template?.Replace("___NEWDB___", tenant!.DatabaseName);

            return connectionString!;
        }
    }

}
