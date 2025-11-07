using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Refit;
using WoopiAiHub.Domain.Interfaces.Refit;
using WoopiAiHub.Domain.Interfaces.Refit.Functions;
using WoopiAiHub.Domain.Utils;
using WoopiAiHub.Infrastructure.Messaging.Configuration;

namespace WoopiAiHub.Application.DependencyInjection
{
    public static class ExternalApi
    {
        public static IServiceCollection AddExternalApi(this IServiceCollection services, IConfiguration configuration)
        {
            var externalSettingsSection = configuration.GetSection(nameof(RefitExternalSettings));
            var externalSettings = externalSettingsSection.Get<RefitExternalSettings>();

            if (string.IsNullOrWhiteSpace(externalSettings.EmbeddingsApiBaseAddress))
                throw new ArgumentNullException($"{nameof(RefitExternalSettings)}_{nameof(externalSettings.EmbeddingsApiBaseAddress)}");

            else if (string.IsNullOrWhiteSpace(externalSettings.FileRepositoryApiBaseAddress))
                throw new ArgumentNullException($"{nameof(RefitExternalSettings)}_{nameof(externalSettings.FileRepositoryApiBaseAddress)}");

            else if (string.IsNullOrWhiteSpace(externalSettings.FunctionGetFileBaseAddress))
                throw new ArgumentNullException($"{nameof(RefitExternalSettings)}_{nameof(externalSettings.FunctionGetFileBaseAddress)}");

            else if (string.IsNullOrWhiteSpace(externalSettings.GraphApiBaseAddress))
                throw new ArgumentNullException($"{nameof(RefitExternalSettings)}_{nameof(externalSettings.GraphApiBaseAddress)}");

            else if (string.IsNullOrWhiteSpace(externalSettings.MarketPlaceBaseAddress))
                throw new ArgumentNullException($"{nameof(RefitExternalSettings)}_{nameof(externalSettings.MarketPlaceBaseAddress)}");

            else if (string.IsNullOrWhiteSpace(externalSettings.KeyGeneratorApiBaseAddress))
                throw new ArgumentNullException($"{nameof(RefitExternalSettings)}_{nameof(externalSettings.KeyGeneratorApiBaseAddress)}");


            services.AddRefitClient<IEmbeddingsApi>().ConfigureHttpClient(c => c.BaseAddress = new Uri(externalSettings.EmbeddingsApiBaseAddress));
            services.AddRefitClient<IFileRepositoryApi>().ConfigureHttpClient(c => c.BaseAddress = new Uri(externalSettings.FileRepositoryApiBaseAddress));
            services.AddRefitClient<IFunctionFileRetriever>().ConfigureHttpClient(c => c.BaseAddress = new Uri(externalSettings.FunctionGetFileBaseAddress));
            services.AddRefitClient<IGraphApi>().ConfigureHttpClient(c => c.BaseAddress = new Uri(externalSettings.GraphApiBaseAddress));
            services.AddRefitClient<IMarketPlaceApi>().ConfigureHttpClient(c => c.BaseAddress = new Uri(externalSettings.MarketPlaceBaseAddress));
            services.AddRefitClient<IKeyGeneratorApi>().ConfigureHttpClient(c => c.BaseAddress = new Uri(externalSettings.KeyGeneratorApiBaseAddress));

            services.Configure<EncryptionSettings>(configuration.GetSection("EncryptionSettings"));

            return services;
        }
    }
}