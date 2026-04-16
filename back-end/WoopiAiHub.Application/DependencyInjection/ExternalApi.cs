using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Refit;
using WoopiAiHub.Domain.Interfaces.Refit;
using WoopiAiHub.Domain.Interfaces.Refit.Functions;
using WoopiAiHub.Domain.Utils;

namespace WoopiAiHub.Application.DependencyInjection
{
    public static class ExternalApi
    {
        public static IServiceCollection AddExternalApi(this IServiceCollection services, IConfiguration configuration)
        {
            var externalSettingsSection = configuration.GetSection(nameof(RefitExternalSettings));
            var externalSettings = externalSettingsSection.Get<RefitExternalSettings>();

            if (string.IsNullOrWhiteSpace(externalSettings.IndexerApiBaseAddress))
                throw new ArgumentNullException($"{nameof(RefitExternalSettings)}_{nameof(externalSettings.IndexerApiBaseAddress)}");

            else if (string.IsNullOrWhiteSpace(externalSettings.FileRepositoryApiBaseAddress))
                throw new ArgumentNullException($"{nameof(RefitExternalSettings)}_{nameof(externalSettings.FileRepositoryApiBaseAddress)}");

            else if (string.IsNullOrWhiteSpace(externalSettings.FunctionGetFileBaseAddress))
                throw new ArgumentNullException($"{nameof(RefitExternalSettings)}_{nameof(externalSettings.FunctionGetFileBaseAddress)}");

            else if (string.IsNullOrWhiteSpace(externalSettings.GraphApiBaseAddress))
                throw new ArgumentNullException($"{nameof(RefitExternalSettings)}_{nameof(externalSettings.GraphApiBaseAddress)}");

            else if (string.IsNullOrWhiteSpace(externalSettings.MarketPlaceBaseAddress))
                throw new ArgumentNullException($"{nameof(RefitExternalSettings)}_{nameof(externalSettings.MarketPlaceBaseAddress)}");

            else if (string.IsNullOrWhiteSpace(externalSettings.AiGatewayApiBaseAddress))
                throw new ArgumentNullException($"{nameof(RefitExternalSettings)}_{nameof(externalSettings.AiGatewayApiBaseAddress)}");

            else if (string.IsNullOrWhiteSpace(externalSettings.IntegrationApiBaseAddress))
                throw new ArgumentNullException($"{nameof(RefitExternalSettings)}_{nameof(externalSettings.IntegrationApiBaseAddress)}");

            services.AddRefitClient<IEmbeddingsApi>().ConfigureHttpClient(c => c.BaseAddress = new Uri(externalSettings.IndexerApiBaseAddress));
            services.AddRefitClient<IFileRepositoryApi>().ConfigureHttpClient(c => c.BaseAddress = new Uri(externalSettings.FileRepositoryApiBaseAddress));
            services.AddRefitClient<IFunctionFileRetriever>().ConfigureHttpClient(c => c.BaseAddress = new Uri(externalSettings.FunctionGetFileBaseAddress));
            services.AddRefitClient<IGraphApi>().ConfigureHttpClient(c => c.BaseAddress = new Uri(externalSettings.GraphApiBaseAddress));
            services.AddRefitClient<IMarketPlaceApi>().ConfigureHttpClient(c => c.BaseAddress = new Uri(externalSettings.MarketPlaceBaseAddress));
            services.AddRefitClient<IChatCompletionApi>().ConfigureHttpClient(c => c.BaseAddress = new Uri(externalSettings.AiGatewayApiBaseAddress));
            services.AddRefitClient<IAzureAiSearch>().ConfigureHttpClient(c => c.BaseAddress = new Uri(externalSettings.IntegrationApiBaseAddress));

            services.Configure<EncryptionSettings>(configuration.GetSection("EncryptionSettings"));
            services.Configure<PromptSettings>(configuration.GetSection("PromptSettings"));

            return services;
        }
    }
}
