using DocAnalyzer.Domain.Interfaces.Refit;
using DocAnalyzer.Domain.Interfaces.Utils;

namespace DocAnalyzer.Application.Utils
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
