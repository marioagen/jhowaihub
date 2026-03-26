using System.Net.Http.Headers;
using WoopiAiHub.Domain.Interfaces.ApiTemplateRequestTests;

namespace WoopiAiHub.Application.Utils
{
    public class ApiTemplateRequestTestsHttpGateway(IHttpClientFactory httpClientFactory) : IApiTemplateRequestTestsHttpGateway
    {
        public const string NamedClient = "ApiTemplateRequestTests";

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

        private async Task<HttpResponseMessage> SendAsync(
            HttpMethod method,
            string url,
            HttpContent? body,
            Dictionary<string, string>? headers,
            CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(url);

            using var request = new HttpRequestMessage(method, url);
            if (body != null)
                request.Content = body;

            ApplyHeaders(request, body, headers);

            var client = _httpClientFactory.CreateClient(NamedClient);
            return await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
        }

        private static void ApplyHeaders(HttpRequestMessage request, HttpContent? body, Dictionary<string, string>? headers)
        {
            if (headers == null || headers.Count == 0)
                return;

            foreach (var (name, value) in headers)
            {
                if (string.IsNullOrEmpty(name) || string.Equals(name, "Host", StringComparison.OrdinalIgnoreCase))
                    continue;

                if (string.Equals(name, "Content-Type", StringComparison.OrdinalIgnoreCase) && body != null)
                {
                    if (MediaTypeHeaderValue.TryParse(value, out var mediaType))
                        body.Headers.ContentType = mediaType;
                    continue;
                }

                request.Headers.TryAddWithoutValidation(name, value);
            }
        }
    }
}
