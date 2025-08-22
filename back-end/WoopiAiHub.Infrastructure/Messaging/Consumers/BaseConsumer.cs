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
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IConfiguration _configuration;

        protected BaseConsumer(
            IServiceScopeFactory scopeFactory,
            IConfiguration configuration)
        {
            _scopeFactory = scopeFactory;
            _configuration = configuration;
        }

        /// <summary>
        /// Configure the tenant context based on the message and module.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="module"></param>
        /// <param name="tenantSelector"></param>
        /// <returns></returns>
        //protected async Task ConfigureTenantContextAsync(TMessage message,
        //                                                 ColTypeModule module,
        //                                                 TenantSelector<TMessage> tenantSelector)
        protected async Task ConfigureTenantContextAsync(string temantName,
                                                         ColTypeModule module)
        {
            using var scope = _scopeFactory.CreateScope();

            var tenantCacheService = scope.ServiceProvider.GetRequiredService<ITenantCacheServices>();
            var httpAccessor = scope.ServiceProvider.GetRequiredService<IHttpContextAccessor>();

            var tenant = await tenantCacheService.FindTenantAsync(temantName, module);

            httpAccessor.HttpContext ??= new DefaultHttpContext();
            var template = _configuration.GetConnectionString("TemplateConnection");
            var connectionString = template?.Replace("___NEWDB___", tenant!.DatabaseName);
            httpAccessor.HttpContext.Items["TenantConnection"] = connectionString;
        }
    }

}
