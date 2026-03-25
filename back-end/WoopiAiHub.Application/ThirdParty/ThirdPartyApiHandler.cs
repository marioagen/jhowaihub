using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using Microsoft.AspNetCore.WebUtilities;
using WoopiAiHub.Domain.DTOs.Request;
using WoopiAiHub.Domain.DTOs.Response;
using WoopiAiHub.Domain.Interfaces.ThirdParty;

namespace WoopiAiHub.Application.ThirdParty
{
    public class ThirdPartyApiHandler(IHttpRequestGateway httpRequestGateway) : IThirdPartyApiHandler
    {
        private readonly IHttpRequestGateway _httpRequestGateway = httpRequestGateway;
        private readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNameCaseInsensitive = true,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };

        public async Task<ThirdPartyApiResponseDto> ExecuteAsync(ThirdPartyApiRequestDto request, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(request);
            if (string.IsNullOrWhiteSpace(request.Url))
                throw new InvalidOperationException("Url is required and must be an absolute URI.");
            if (!Uri.TryCreate(request.Url, UriKind.Absolute, out _))
                throw new InvalidOperationException("Url must be an absolute URI.");

            var url = BuildFinalUrl(request.Url, request.Query);
            var method = string.IsNullOrWhiteSpace(request.Method)
                ? string.Empty
                : request.Method.Trim().ToUpperInvariant();

            using HttpResponseMessage response = method switch
            {
                "GET" => await _httpRequestGateway.GetAsync(url, request.Headers, cancellationToken),
                "POST" => await _httpRequestGateway.PostAsync(
                    url,
                    BuildJsonHttpContent(request.Body),
                    request.Headers,
                    cancellationToken),
                "PUT" => await _httpRequestGateway.PutAsync(
                    url,
                    BuildJsonHttpContent(request.Body),
                    request.Headers,
                    cancellationToken),
                "PATCH" => await _httpRequestGateway.PatchAsync(
                    url,
                    BuildJsonHttpContent(request.Body),
                    request.Headers,
                    cancellationToken),
                "DELETE" => await _httpRequestGateway.DeleteAsync(url, request.Headers, cancellationToken),
                _ => throw new InvalidOperationException($"HTTP method '{request.Method}' is not supported. Use GET, POST, PUT, PATCH, or DELETE.")
            };

            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            return new ThirdPartyApiResponseDto
            {
                StatusCode = (int)response.StatusCode,
                Content = content,
                TemplateName = request.TemplateName,
                Tenant = request.Tenant,
                Email = request.Email,
                ExecutionId = request.ExecutionId
            };
        }

        private static string BuildFinalUrl(string url, Dictionary<string, string>? query)
        {
            if (query == null || query.Count == 0)
                return url;
            var forQuery = query.ToDictionary(kv => kv.Key, kv => (string?)kv.Value);
            return QueryHelpers.AddQueryString(url, forQuery);
        }

        private HttpContent? BuildJsonHttpContent(string? body)
        {
            if (body is null)
                return null;

            var jsonBody = UnwrapDoubleEncodedJsonStringIfNeeded(body);
            return new StringContent(jsonBody, Encoding.UTF8, "application/json");
        }

        private string UnwrapDoubleEncodedJsonStringIfNeeded(string body)
        {
            var trimmed = body.Trim();
            if (trimmed.Length < 2 || trimmed[0] != '"' || trimmed[^1] != '"')
                return body;

            try
            {
                var unwrapped = JsonSerializer.Deserialize<string>(trimmed, _jsonOptions);
                if (unwrapped is null)
                    throw new InvalidOperationException("Body is a JSON string literal but deserialized to null.");
                return unwrapped;
            }
            catch (JsonException ex)
            {
                throw new InvalidOperationException(
                    "Body appears to be a double-encoded JSON string (wrapped in quotes) but could not be unwrapped. Ensure it is valid JSON.",
                    ex);
            }
        }
    }
}
