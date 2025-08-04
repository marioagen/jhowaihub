using Microsoft.Extensions.DependencyInjection;
using WoopiAiHub.Infrastructure.Multitenancy;

namespace WoopiAiHub.Infrastructure.DependencyInjection
{
    public static class Extension
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services)
        {
            services.AddScoped<ITenantContextService, TenantContextService>();
            services.AddHttpContextAccessor();

            return services;
        }
    }
}
