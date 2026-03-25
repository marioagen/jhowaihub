using WoopiAiHub.Domain.Interfaces.ThirdParty;

namespace WoopiAiHub.Application.Utils
{
    public class HttpRequestGateway(IHttpClientFactory httpClientFactory) : IHttpRequestGateway
    {
        public const string NamedClient = "ThirdPartyIntegration";

        private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;

        public Task<HttpResponseMessage> GetAsync(string url, Dictionary<string, string>? headers, CancellationToken cancellationToken) =>
            SendAsync(HttpMethod.Get, url, body: null, headers, cancellationToken);

        public Task<HttpResponseMessage> PostAsync(string url, HttpContent? body, Dictionary<string, string>? headers, CancellationToken cancellationToken) =>
            SendAsync(HttpMethod.Post, url, body, headers, cancellationToken);

        public Task<HttpResponseMessage> PutAsync(string url, HttpContent? body, Dictionary<string, string>? headers, CancellationToken cancellationToken) =>
            SendAsync(HttpMethod.Put, url, body, headers, cancellationToken);

        public Task<HttpResponseMessage> PatchAsync(string url, HttpContent? body, Dictionary<string, string>? headers, CancellationToken cancellationToken) =>
            SendAsync(HttpMethod.Patch, url, body, headers, cancellationToken);

        public Task<HttpResponseMessage> DeleteAsync(string url, Dictionary<string, string>? headers, CancellationToken cancellationToken) =>
            SendAsync(HttpMethod.Delete, url, body: null, headers, cancellationToken);

        private Task<HttpResponseMessage> SendAsync(
            HttpMethod method,
            string url,
            HttpContent? body,
            Dictionary<string, string>? headers,
            CancellationToken cancellationToken)
        {
            var uri = new Uri(url);
            var baseAddress = $"{uri.Scheme}://{uri.Authority}";
            using var client = _httpClientFactory.CreateClient(NamedClient);
            client.BaseAddress = new Uri(baseAddress);
            ApplyHeaders(client, headers);

            if (method == HttpMethod.Get || method == HttpMethod.Delete)
            {
                return method == HttpMethod.Get
                    ? client.GetAsync(url, cancellationToken)
                    : client.DeleteAsync(url, cancellationToken);
            }

            if (method == HttpMethod.Post)
                return client.PostAsync(url, body, cancellationToken);
            if (method == HttpMethod.Put)
                return client.PutAsync(url, body, cancellationToken);
            if (method == HttpMethod.Patch)
                return client.PatchAsync(url, body, cancellationToken);

            throw new InvalidOperationException($"Unsupported HTTP method: {method}");
        }

        private static void ApplyHeaders(HttpClient client, Dictionary<string, string>? headers)
        {
            if (headers == null || headers.Count == 0)
                return;

            foreach (var (name, value) in headers)
            {
                if (string.IsNullOrEmpty(name) || string.Equals(name, "Host", StringComparison.OrdinalIgnoreCase))
                    continue;
                client.DefaultRequestHeaders.TryAddWithoutValidation(name, value);
            }
        }
    }
}
