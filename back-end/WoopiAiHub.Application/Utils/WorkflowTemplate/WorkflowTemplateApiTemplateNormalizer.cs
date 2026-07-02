using System.Text.Json;
using System.Text.Json.Nodes;

namespace WoopiAiHub.Application.Utils.WorkflowTemplate
{
    public static class WorkflowTemplateApiTemplateNormalizer
    {
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };

        /// <summary>
        /// Normalizes query/header templates to the array format expected by the UI
        /// ([{ "key": "...", "value": "..." }]). Object maps are converted; arrays pass through.
        /// </summary>
        public static string? NormalizeKeyValueTemplate(string? template)
        {
            if (string.IsNullOrWhiteSpace(template))
                return template;

            try
            {
                using var doc = JsonDocument.Parse(template);
                return doc.RootElement.ValueKind switch
                {
                    JsonValueKind.Array => template,
                    JsonValueKind.Object => ConvertObjectToKeyValueArray(doc.RootElement),
                    _ => template
                };
            }
            catch (JsonException)
            {
                return template;
            }
        }

        private static string ConvertObjectToKeyValueArray(JsonElement obj)
        {
            var array = new JsonArray();
            foreach (var prop in obj.EnumerateObject())
            {
                array.Add(new JsonObject
                {
                    ["key"] = prop.Name,
                    ["value"] = prop.Value.ValueKind switch
                    {
                        JsonValueKind.String => prop.Value.GetString(),
                        JsonValueKind.Null => null,
                        _ => prop.Value.GetRawText()
                    }
                });
            }

            return array.ToJsonString(JsonOptions);
        }
    }
}
