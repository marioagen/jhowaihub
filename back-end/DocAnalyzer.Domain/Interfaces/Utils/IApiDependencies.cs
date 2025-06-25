using DocAnalyzer.Domain.Interfaces.Refit;
using DocAnalyzer.Domain.Utils;

namespace DocAnalyzer.Domain.Interfaces.Utils
{
    public interface IApiDependencies
    {
        IMarketPlaceApi MarketPlaceApi { get; }
        IKeyGeneratorApi KeyGeneratorApi { get; }
    }
}
