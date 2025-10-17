using WoopiAiHub.Domain.Interfaces.Refit;

namespace WoopiAiHub.Domain.Interfaces.Utils
{
    public interface IApiClientFactory
    {
        In8NConnector Create(string baseUrl);
    }
}
