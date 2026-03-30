using System.Net.Http.Headers;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using WoopiAiHub.Domain.DTOs.Request;
using WoopiAiHub.Domain.DTOs.Response;
using WoopiAiHub.Domain.Interfaces.ApiTemplateRequestTests;
using WoopiAiHub.Domain.Interfaces.Repository;

namespace WoopiAiHub.Application.ApiTemplateRequestTests
{
    public class ApiTemplateRequestTestsHandler(
        IApiTemplateRequestTestsHttpGateway httpGateway,
        IApiTemplateRepository apiTemplateRepository) : IApiTemplateRequestTestsHandler
    {
        private readonly IApiTemplateRequestTestsHttpGateway _httpGateway = httpGateway;
        private readonly IApiTemplateRepository _apiTemplateRepository = apiTemplateRepository;
        private readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNameCaseInsensitive = true,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };

        /// <summary>
        /// Assembles the HTTP request from the template draft (or persisted template), sends it via the gateway, and returns status, body, and metadata.
        /// </summary>
        /// <param name="request">The test request containing variables, optional inline draft or template id, and correlation fields.</param>
        /// <param name="cancellationToken">Token to cancel the operation.</param>
        /// <returns>The HTTP status code, response content, and echoed template context.</returns>
        public async Task<ApiTemplateRequestTestsResponseDto> ExecuteAsync(ApiTemplateRequestTestsRequestDto request, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(request);

            var variables = request.Variables ?? new Dictionary<string, string>(StringComparer.Ordinal);
            var draft = await ResolveDraftAsync(request).ConfigureAwait(false);

            var assembled = ApiTemplateRequestTestsRequestAssembler.Assemble(
                draft.Method,
                draft.Url,
                draft.QueryTemplate,
                draft.HeaderTemplate,
                draft.BodyTemplate,
                variables);

            var method = assembled.Method;
            var url = assembled.Url;
            var headers = assembled.Headers;

            using HttpResponseMessage response = method switch
            {
                "GET" => await _httpGateway.GetAsync(url, headers, cancellationToken).ConfigureAwait(false),
                "POST" => await _httpGateway.PostAsync(
                    url,
                    BuildJsonHttpContent(assembled.Body),
                    headers,
                    cancellationToken).ConfigureAwait(false),
                "PUT" => await _httpGateway.PutAsync(
                    url,
                    BuildJsonHttpContent(assembled.Body),
                    headers,
                    cancellationToken).ConfigureAwait(false),
                "PATCH" => await _httpGateway.PatchAsync(
                    url,
                    BuildJsonHttpContent(assembled.Body),
                    headers,
                    cancellationToken).ConfigureAwait(false),
                "DELETE" => await _httpGateway.DeleteAsync(url, headers, cancellationToken).ConfigureAwait(false),
                _ => throw new InvalidOperationException($"HTTP method '{method}' is not supported. Use GET, POST, PUT, PATCH, or DELETE.")
            };

            var content = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
            return new ApiTemplateRequestTestsResponseDto
            {
                StatusCode = (int)response.StatusCode,
                Content = content,
                TemplateName = request.TemplateName ?? draft.Name,
                Tenant = request.Tenant,
                Email = request.Email,
                ExecutionId = request.ExecutionId
            };
        }

        /// <summary>
        /// Returns the inline draft from the request, or loads and maps the API template by <see cref="ApiTemplateRequestTestsRequestDto.TemplateId"/>.
        /// </summary>
        private async Task<ApiTemplateCreateDto> ResolveDraftAsync(ApiTemplateRequestTestsRequestDto request)
        {
            if (request.Draft != null)
                return request.Draft;

            if (request.TemplateId is int id && id > 0)
            {
                var model = await _apiTemplateRepository.FindByIdReturnModel(id).ConfigureAwait(false);
                if (model == null)
                    throw new InvalidOperationException($"API template with id '{id}' was not found.");

                return ApiTemplateRequestTestsRequestAssembler.ToDraft(model);
            }

            throw new InvalidOperationException("Either Draft or a valid TemplateId must be provided.");
        }

        /// <summary>
        /// Creates UTF-8 JSON <see cref="StringContent"/> for the request body, or <c>null</c> when there is no body.
        /// </summary>
        private StringContent? BuildJsonHttpContent(string? body)
        {
            if (body is null)
                return null;

            var jsonBody = UnwrapDoubleEncodedJsonStringIfNeeded(body);
            return new StringContent(jsonBody, Encoding.UTF8, "application/json");
        }

        /// <summary>
        /// If <paramref name="body"/> is a JSON string literal (wrapped in quotes), returns the inner string; otherwise returns <paramref name="body"/> unchanged.
        /// </summary>
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
