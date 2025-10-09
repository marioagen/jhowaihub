using System.Text.Json.Nodes;
using WoopiAiHub.Domain.DTOs.Connector;

namespace WoopiAiHub.Application.Utils
{
    public static class JsonSchemaToFormMapper
    {
        public static List<FormFieldDto> MapToFormFields(string json)
        {
            var schema = JsonNode.Parse(json);
            var properties = schema?["properties"]?.AsObject();
            var required = schema?["required"]?.AsArray()?.Select(x => x.ToString()).ToList() ?? new();

            var result = new List<FormFieldDto>();
            if (properties != null)
            {
                foreach (var prop in properties)
                    result.Add(MapField(prop.Key, prop.Value, required));
            }

            return result;
        }

        private static FormFieldDto MapField(string name, JsonNode? node, List<string> required)
        {
            var type = node?["type"]?.ToString() ?? "object";
            var max = node?["maxLength"]?.GetValue<int>() ?? null;
            var min = node?["minLength"]?.GetValue<int>() ?? null;
            var description = node?["description"]?.ToString() ?? "";
            var isRequired = required.Contains(name);

            var field = new FormFieldDto
            {
                Name = name,
                Type = type,
                Label = description,
                Required = isRequired,
                MaxLength = max,
                MinLength = min,
                Children = new()
            };

            if (node?["properties"] != null)
            {
                var subRequired = node?["required"]?.AsArray()?.Select(x => x.ToString()).ToList() ?? new();
                foreach (var sub in node["properties"]!.AsObject())
                    field.Children!.Add(MapField(sub.Key, sub.Value, subRequired));
            }

            if (type == "array" && node?["items"]?["properties"] != null)
            {
                var subRequired = node["items"]?["required"]?.AsArray()?.Select(x => x.ToString()).ToList() ?? new();
                foreach (var sub in node["items"]!["properties"]!.AsObject())
                    field.Children!.Add(MapField(sub.Key, sub.Value, subRequired));
            }

            return field;
        }
    }
}
