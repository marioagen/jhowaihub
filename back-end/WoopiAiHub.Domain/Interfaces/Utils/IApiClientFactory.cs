using WoopiAiHub.Domain.Interfaces.Refit;

namespace WoopiAiHub.Domain.Interfaces.Utils
{
    public interface IApiClientFactory
    {
        In8nConnector Create(string baseUrl);
    }
}
