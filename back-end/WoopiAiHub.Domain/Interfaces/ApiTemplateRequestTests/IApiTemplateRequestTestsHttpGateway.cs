namespace WoopiAiHub.Domain.Interfaces.ApiTemplateRequestTests
{
    /// <summary>
    /// Abstraction for executing HTTP calls during API template request tests.
    /// </summary>
    public interface IApiTemplateRequestTestsHttpGateway
    {
        Task<HttpResponseMessage> GetAsync(string url, Dictionary<string, string>? headers, CancellationToken cancellationToken);
        Task<HttpResponseMessage> PostAsync(string url, HttpContent? body, Dictionary<string, string>? headers, CancellationToken cancellationToken);
        Task<HttpResponseMessage> PutAsync(string url, HttpContent? body, Dictionary<string, string>? headers, CancellationToken cancellationToken);
        Task<HttpResponseMessage> PatchAsync(string url, HttpContent? body, Dictionary<string, string>? headers, CancellationToken cancellationToken);
        Task<HttpResponseMessage> DeleteAsync(string url, Dictionary<string, string>? headers, CancellationToken cancellationToken);
    }
}
