using System.Net.Http.Headers;
using WoopiAiHub.Domain.Interfaces.ApiTemplateRequestCheck;

namespace WoopiAiHub.Application.Utils
{
    /// <summary>
    /// Sends HTTP requests for API template dry-runs using a named <see cref="IHttpClientFactory"/> client.
    /// </summary>
    public class ApiTemplateRequestCheckHttpGateway(IHttpClientFactory httpClientFactory) : IApiTemplateRequestCheckHttpGateway
    {
        /// <summary>
        /// Name passed to <see cref="IHttpClientFactory.CreateClient(string)"/> for the template check HTTP client.
        /// </summary>
        public const string NamedClient = "ApiTemplateRequestCheck";

        private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;

        /// <inheritdoc />
        public Task<HttpResponseMessage> GetAsync(string url, Dictionary<string, string>? headers, CancellationToken cancellationToken) =>
            SendAsync(HttpMethod.Get, url, body: null, headers, cancellationToken);

        /// <inheritdoc />
        public Task<HttpResponseMessage> PostAsync(string url, HttpContent? body, Dictionary<string, string>? headers, CancellationToken cancellationToken) =>
            SendAsync(HttpMethod.Post, url, body, headers, cancellationToken);

        /// <inheritdoc />
        public Task<HttpResponseMessage> PutAsync(string url, HttpContent? body, Dictionary<string, string>? headers, CancellationToken cancellationToken) =>
            SendAsync(HttpMethod.Put, url, body, headers, cancellationToken);

        /// <inheritdoc />
        public Task<HttpResponseMessage> PatchAsync(string url, HttpContent? body, Dictionary<string, string>? headers, CancellationToken cancellationToken) =>
            SendAsync(HttpMethod.Patch, url, body, headers, cancellationToken);

        /// <inheritdoc />
        public Task<HttpResponseMessage> DeleteAsync(string url, Dictionary<string, string>? headers, CancellationToken cancellationToken) =>
            SendAsync(HttpMethod.Delete, url, body: null, headers, cancellationToken);

        /// <summary>
        /// Builds the request, applies headers, and sends it with response headers read as the completion boundary.
        /// </summary>
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

        /// <summary>
        /// Adds custom headers to the request; maps <c>Content-Type</c> to <see cref="HttpContent.Headers"/> when a body is present. Skips <c>Host</c>.
        /// </summary>
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
