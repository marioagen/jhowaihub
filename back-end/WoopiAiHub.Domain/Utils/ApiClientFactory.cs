using Refit;
using WoopiAiHub.Domain.Interfaces.Refit;
using WoopiAiHub.Domain.Interfaces.Utils;

namespace WoopiAiHub.Domain.Utils
{
    public class ApiClientFactory : IApiClientFactory
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public ApiClientFactory(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        /// <summary>
        /// Creates a client for In8nConnector
        /// </summary>
        /// <param name="baseUrl"></param>
        /// <returns></returns>
        public In8nConnector Create(string baseUrl)
        {
            var client = _httpClientFactory.CreateClient("WoopiAi Hub");
            client.BaseAddress = new Uri(baseUrl);
            return RestService.For<In8nConnector>(client);
        }
    }
}
