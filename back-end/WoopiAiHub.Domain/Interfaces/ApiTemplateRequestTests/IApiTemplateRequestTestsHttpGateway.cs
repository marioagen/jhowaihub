namespace WoopiAiHub.Domain.Interfaces.ApiTemplateRequestTests
{
    /// <summary>
    /// Abstraction for executing HTTP calls during API template request tests.
    /// </summary>
    public interface IApiTemplateRequestTestsHttpGateway
    {
        /// <summary>
        /// Sends an HTTP GET to <paramref name="url"/> with optional <paramref name="headers"/>.
        /// </summary>
        /// <param name="url">Request URL. Cannot be null.</param>
        /// <param name="headers">Optional header name/value pairs.</param>
        /// <param name="cancellationToken">Token to cancel the operation.</param>
        /// <returns>The HTTP response message.</returns>
        Task<HttpResponseMessage> GetAsync(string url, Dictionary<string, string>? headers, CancellationToken cancellationToken);

        /// <summary>
        /// Sends an HTTP POST to <paramref name="url"/> with optional <paramref name="body"/> and <paramref name="headers"/>.
        /// </summary>
        /// <param name="url">Request URL. Cannot be null.</param>
        /// <param name="body">Optional request content.</param>
        /// <param name="headers">Optional header name/value pairs.</param>
        /// <param name="cancellationToken">Token to cancel the operation.</param>
        /// <returns>The HTTP response message.</returns>
        Task<HttpResponseMessage> PostAsync(string url, HttpContent? body, Dictionary<string, string>? headers, CancellationToken cancellationToken);

        /// <summary>
        /// Sends an HTTP PUT to <paramref name="url"/> with optional <paramref name="body"/> and <paramref name="headers"/>.
        /// </summary>
        /// <param name="url">Request URL. Cannot be null.</param>
        /// <param name="body">Optional request content.</param>
        /// <param name="headers">Optional header name/value pairs.</param>
        /// <param name="cancellationToken">Token to cancel the operation.</param>
        /// <returns>The HTTP response message.</returns>
        Task<HttpResponseMessage> PutAsync(string url, HttpContent? body, Dictionary<string, string>? headers, CancellationToken cancellationToken);

        /// <summary>
        /// Sends an HTTP PATCH to <paramref name="url"/> with optional <paramref name="body"/> and <paramref name="headers"/>.
        /// </summary>
        /// <param name="url">Request URL. Cannot be null.</param>
        /// <param name="body">Optional request content.</param>
        /// <param name="headers">Optional header name/value pairs.</param>
        /// <param name="cancellationToken">Token to cancel the operation.</param>
        /// <returns>The HTTP response message.</returns>
        Task<HttpResponseMessage> PatchAsync(string url, HttpContent? body, Dictionary<string, string>? headers, CancellationToken cancellationToken);

        /// <summary>
        /// Sends an HTTP DELETE to <paramref name="url"/> with optional <paramref name="headers"/>.
        /// </summary>
        /// <param name="url">Request URL. Cannot be null.</param>
        /// <param name="headers">Optional header name/value pairs.</param>
        /// <param name="cancellationToken">Token to cancel the operation.</param>
        /// <returns>The HTTP response message.</returns>
        Task<HttpResponseMessage> DeleteAsync(string url, Dictionary<string, string>? headers, CancellationToken cancellationToken);
    }
}
