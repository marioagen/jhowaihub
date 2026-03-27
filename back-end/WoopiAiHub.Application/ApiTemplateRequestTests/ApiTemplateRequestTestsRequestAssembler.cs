using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.WebUtilities;
using WoopiAiHub.Domain.DTOs.Request;
using WoopiAiHub.Domain.Models;

namespace WoopiAiHub.Application.ApiTemplateRequestTests
{
    internal static class ApiTemplateRequestTestsRequestAssembler
    {
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        internal sealed record AssembledOutboundRequest(
            string Method,
            string Url,
            Dictionary<string, string>? Headers,
            string? Body);

        internal static AssembledOutboundRequest Assemble(
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

            return new AssembledOutboundRequest(methodNormalized, finalUrl, headers, bodyText);
        }

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

        private static string MergeQueryTemplate(string urlAfterSubstitute, string? queryTemplateJson)
        {
            if (string.IsNullOrWhiteSpace(queryTemplateJson))
                return urlAfterSubstitute;

            List<JsonKeyValue>? items;
            try
            {
                items = JsonSerializer.Deserialize<List<JsonKeyValue>>(queryTemplateJson, JsonOptions);
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

        private static Dictionary<string, string>? ParseHeaderTemplate(string? headerTemplateJson)
        {
            if (string.IsNullOrWhiteSpace(headerTemplateJson))
                return null;

            List<JsonKeyValue>? items;
            try
            {
                items = JsonSerializer.Deserialize<List<JsonKeyValue>>(headerTemplateJson, JsonOptions);
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

        private sealed record JsonKeyValue(string Key, string Value);

        internal static ApiTemplateCreateDto ToDraft(ApiTemplate model) =>
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
