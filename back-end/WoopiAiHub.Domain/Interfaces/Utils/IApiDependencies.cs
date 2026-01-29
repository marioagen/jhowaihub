using WoopiAiHub.Domain.Interfaces.Refit;
using WoopiAiHub.Domain.Utils;

namespace WoopiAiHub.Domain.Interfaces.Utils
{
    public interface IApiDependencies
    {
        IMarketPlaceApi MarketPlaceApi { get; }
    }
}
