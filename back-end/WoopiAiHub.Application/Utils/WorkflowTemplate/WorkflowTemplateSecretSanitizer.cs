using System.Text.Json;
using System.Text.RegularExpressions;

namespace WoopiAiHub.Application.Utils.WorkflowTemplate
{
    public static class WorkflowTemplateSecretSanitizer
    {
        public const string SecretPrefix = "{{SECRET:";
        public const string SecretSuffix = "}}";

        private static readonly HashSet<string> RuntimePlaceholders = new(StringComparer.OrdinalIgnoreCase)
        {
            "ocr", "embeddings", "prompt", "referenceFile", "timestamp", "token"
        };

        private static readonly HashSet<string> SensitiveJsonKeys = new(StringComparer.OrdinalIgnoreCase)
        {
            "authorization", "api_key", "apikey", "api-key", "x-api-key",
            "client_secret", "clientsecret", "password", "secret", "access_token", "refresh_token"
        };

        private static readonly Regex BearerTokenRegex = new(
            @"Bearer\s+[A-Za-z0-9\-._~+/]+=*",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        public static (string Sanitized, HashSet<string> Secrets) SanitizeText(string? text, string secretKeyBase)
        {
            var secrets = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            if (string.IsNullOrWhiteSpace(text))
                return (text ?? string.Empty, secrets);

            var result = SanitizeBearerTokens(text, secretKeyBase, secrets);
            result = SanitizeJsonStringValues(result, secretKeyBase, secrets);
            return (result, secrets);
        }

        public static string ApplySecrets(string? text, IReadOnlyDictionary<string, string> secretValues)
        {
            if (string.IsNullOrWhiteSpace(text))
                return text ?? string.Empty;

            var result = text;
            foreach (var (key, value) in secretValues)
            {
                if (string.IsNullOrEmpty(key))
                    continue;
                result = result.Replace($"{SecretPrefix}{key}{SecretSuffix}", value, StringComparison.OrdinalIgnoreCase);
            }

            return result;
        }

        public static List<string> FindUnresolvedSecrets(string? text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return [];

            return Regex.Matches(text, Regex.Escape(SecretPrefix) + @"([A-Za-z0-9_\-]+)" + Regex.Escape(SecretSuffix))
                .Select(m => m.Groups[1].Value)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        public static List<string> FindUnresolvedSecretsInPackage(
            IEnumerable<string?> texts)
        {
            return texts
                .SelectMany(FindUnresolvedSecrets)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(x => x)
                .ToList();
        }

        private static string SanitizeBearerTokens(string text, string secretKeyBase, HashSet<string> secrets)
        {
            return BearerTokenRegex.Replace(text, match =>
            {
                var key = $"{secretKeyBase}_bearer";
                secrets.Add(key);
                return $"Bearer {SecretPrefix}{key}{SecretSuffix}";
            });
        }

        private static string SanitizeJsonStringValues(string text, string secretKeyBase, HashSet<string> secrets)
        {
            try
            {
                using var doc = JsonDocument.Parse(text);
                var root = doc.RootElement;
                if (root.ValueKind is not (JsonValueKind.Object or JsonValueKind.Array))
                    return text;

                using var stream = new MemoryStream();
                using (var writer = new Utf8JsonWriter(stream))
                {
                    WriteSanitizedElement(writer, root, secretKeyBase, secrets);
                }

                return System.Text.Encoding.UTF8.GetString(stream.ToArray());
            }
            catch (JsonException)
            {
                return text;
            }
        }

        private static void WriteSanitizedElement(
            Utf8JsonWriter writer,
            JsonElement element,
            string secretKeyBase,
            HashSet<string> secrets)
        {
            switch (element.ValueKind)
            {
                case JsonValueKind.Object:
                    writer.WriteStartObject();
                    foreach (var prop in element.EnumerateObject())
                    {
                        writer.WritePropertyName(prop.Name);
                        if (prop.Value.ValueKind == JsonValueKind.String
                            && SensitiveJsonKeys.Contains(prop.Name)
                            && ShouldSanitizeStringValue(prop.Value.GetString()))
                        {
                            var key = $"{secretKeyBase}_{SlugKey(prop.Name)}";
                            secrets.Add(key);
                            writer.WriteStringValue($"{SecretPrefix}{key}{SecretSuffix}");
                        }
                        else
                        {
                            WriteSanitizedElement(writer, prop.Value, secretKeyBase, secrets);
                        }
                    }
                    writer.WriteEndObject();
                    break;
                case JsonValueKind.Array:
                    writer.WriteStartArray();
                    foreach (var item in element.EnumerateArray())
                        WriteSanitizedElement(writer, item, secretKeyBase, secrets);
                    writer.WriteEndArray();
                    break;
                default:
                    element.WriteTo(writer);
                    break;
            }
        }

        private static bool ShouldSanitizeStringValue(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return false;

            var trimmed = value.Trim();
            if (trimmed.StartsWith("{{", StringComparison.Ordinal) && trimmed.EndsWith("}}", StringComparison.Ordinal))
            {
                var inner = trimmed[2..^2];
                if (inner.StartsWith("SECRET:", StringComparison.OrdinalIgnoreCase))
                    return false;
                if (RuntimePlaceholders.Contains(inner))
                    return false;
            }

            return trimmed.Length >= 8;
        }

        private static string SlugKey(string name) =>
            Regex.Replace(name, @"[^A-Za-z0-9]+", "_").Trim('_').ToLowerInvariant();
    }
}
