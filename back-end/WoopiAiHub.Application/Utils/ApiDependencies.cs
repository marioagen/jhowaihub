using WoopiAiHub.Domain.Interfaces.Refit;
using WoopiAiHub.Domain.Interfaces.Utils;

namespace WoopiAiHub.Application.Utils
{
    public class ApiDependencies : IApiDependencies
    {
        public IMarketPlaceApi MarketPlaceApi { get; }
        public IKeyGeneratorApi KeyGeneratorApi { get; }

        public ApiDependencies(
            IMarketPlaceApi marketPlaceApi,
            IKeyGeneratorApi keyGeneratorApi)
        {
            MarketPlaceApi = marketPlaceApi;
            KeyGeneratorApi = keyGeneratorApi;
        }
    }
}
