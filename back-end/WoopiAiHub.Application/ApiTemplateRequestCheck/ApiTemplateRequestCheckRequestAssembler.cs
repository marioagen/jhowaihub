using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.WebUtilities;
using WoopiAiHub.Domain.DTOs.Request;
using WoopiAiHub.Domain.Models;

namespace WoopiAiHub.Application.ApiTemplateRequestCheck
{
    /// <summary>
    /// Builds an assembled HTTP request from URL, query, header, and body templates with variable substitution.
    /// </summary>
    public static class ApiTemplateRequestCheckRequestAssembler
    {
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        /// <summary>
        /// Substitutes <c>{{variable}}</c> placeholders, merges query and header JSON templates into the URL and header map, normalizes the HTTP method, and validates the final URL.
        /// </summary>
        /// <param name="method">HTTP method name (trimmed and uppercased in the result).</param>
        /// <param name="url">Base URL template; must become an absolute <c>http</c> or <c>https</c> URI after substitution.</param>
        /// <param name="queryTemplate">Optional JSON array of key/value pairs appended as query string, or <c>null</c>.</param>
        /// <param name="headerTemplate">Optional JSON array of key/value pairs parsed as headers, or <c>null</c>.</param>
        /// <param name="bodyTemplate">Optional body string after variable substitution, or <c>null</c>.</param>
        /// <param name="variables">Values to replace for each <c>{{key}}</c> placeholder.</param>
        /// <returns>Method, final URL, headers, and body ready for the HTTP client.</returns>
        /// <exception cref="InvalidOperationException">Thrown when the URL is not a valid absolute HTTP(S) URI, or query/header JSON is invalid.</exception>
        public static ApiTemplateRequestCheckAssembledRequestDto Assemble(
            string method,
            string url,
            string? queryTemplate,
            string? headerTemplate,
            string? bodyTemplate,
            IReadOnlyDictionary<string, string> variables)
        {
            var urlText = ApplyVariables(url, variables);
            var queryText = queryTemplate == null ? null : ApplyVariables(queryTemplate, variables);
            var headerText = headerTemplate == null ? null : ApplyVariables(headerTemplate, variables);
            var bodyText = bodyTemplate == null ? null : ApplyVariables(bodyTemplate, variables);

            if (!IsHttpOrHttpsAbsoluteUri(urlText))
                throw new InvalidOperationException("URL must be an absolute URI after applying variables.");

            var finalUrl = MergeQueryTemplate(urlText, queryText);

            if (!IsHttpOrHttpsAbsoluteUri(finalUrl))
                throw new InvalidOperationException("Final URL must be an absolute URI.");

            var methodNormalized = string.IsNullOrWhiteSpace(method)
                ? string.Empty
                : method.Trim().ToUpperInvariant();

            var headers = ParseHeaderTemplate(headerText);

            return new ApiTemplateRequestCheckAssembledRequestDto
            {
                Method = methodNormalized,
                Url = finalUrl,
                Headers = headers,
                Body = bodyText
            };
        }

        /// <summary>
        /// Returns whether <paramref name="urlText"/> is a non-empty absolute URI with scheme <c>http</c> or <c>https</c>.
        /// </summary>
        private static bool IsHttpOrHttpsAbsoluteUri(string urlText)
        {
            if (string.IsNullOrWhiteSpace(urlText))
                return false;

            var trimmed = urlText.Trim();
            if (!trimmed.StartsWith("http://", StringComparison.OrdinalIgnoreCase)
                && !trimmed.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                return false;

            return Uri.TryCreate(trimmed, UriKind.Absolute, out var uri)
                && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
        }

        /// <summary>
        /// Replaces every <c>{{key}}</c> in <paramref name="input"/> with the corresponding value from <paramref name="variables"/>.
        /// </summary>
        private static string ApplyVariables(string input, IReadOnlyDictionary<string, string> variables)
        {
            var result = input;
            foreach (var kv in variables)
            {
                var placeholder = "{{" + kv.Key + "}}";
                result = result.Replace(placeholder, kv.Value ?? string.Empty, StringComparison.Ordinal);
            }

            return result;
        }

        /// <summary>
        /// Parses <paramref name="queryTemplateJson"/> as a JSON array of key/value pairs and appends them to <paramref name="urlAfterSubstitute"/>.
        /// </summary>
        private static string MergeQueryTemplate(string urlAfterSubstitute, string? queryTemplateJson)
        {
            if (string.IsNullOrWhiteSpace(queryTemplateJson))
                return urlAfterSubstitute;

            List<ApiTemplateRequestCheckJsonKeyValue>? items;
            try
            {
                items = JsonSerializer.Deserialize<List<ApiTemplateRequestCheckJsonKeyValue>>(queryTemplateJson, JsonOptions);
            }
            catch (JsonException ex)
            {
                throw new InvalidOperationException("Query template is not valid JSON.", ex);
            }

            if (items == null || items.Count == 0)
                return urlAfterSubstitute;

            var dict = new Dictionary<string, string?>(StringComparer.Ordinal);
            foreach (var item in items)
            {
                if (string.IsNullOrWhiteSpace(item.Key))
                    continue;
                dict[item.Key] = item.Value;
            }

            if (dict.Count == 0)
                return urlAfterSubstitute;

            return QueryHelpers.AddQueryString(urlAfterSubstitute, dict);
        }

        /// <summary>
        /// Deserializes <paramref name="headerTemplateJson"/> as a JSON array of key/value pairs into a case-insensitive header dictionary, or <c>null</c> when empty.
        /// </summary>
        private static Dictionary<string, string>? ParseHeaderTemplate(string? headerTemplateJson)
        {
            if (string.IsNullOrWhiteSpace(headerTemplateJson))
                return null;

            List<ApiTemplateRequestCheckJsonKeyValue>? items;
            try
            {
                items = JsonSerializer.Deserialize<List<ApiTemplateRequestCheckJsonKeyValue>>(headerTemplateJson, JsonOptions);
            }
            catch (JsonException ex)
            {
                throw new InvalidOperationException("Header template is not valid JSON.", ex);
            }

            if (items == null || items.Count == 0)
                return null;

            var headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (var item in items)
            {
                if (string.IsNullOrWhiteSpace(item.Key))
                    continue;
                headers[item.Key] = item.Value ?? string.Empty;
            }


            return headers.Count == 0 ? null : headers;
        }

        /// <summary>
        /// Maps a persisted <see cref="ApiTemplate"/> to a create DTO suitable for request assembly (draft shape).
        /// </summary>
        /// <param name="model">The stored API template entity.</param>
        /// <returns>A draft DTO with name, method, URL, and template strings.</returns>
        public static ApiTemplateCreateDto ToDraft(ApiTemplate model) =>
            new()
            {
                Name = model.Name,
                Method = model.Method,
                Url = model.Url,
                QueryTemplate = model.QueryTemplate,
                HeaderTemplate = model.HeaderTemplate,
                BodyTemplate = model.BodyTemplate
            };
    }
}
