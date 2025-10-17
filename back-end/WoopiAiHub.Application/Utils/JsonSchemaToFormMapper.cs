using System.Text.Json.Nodes;
using WoopiAiHub.Domain.DTOs.Connector;

namespace WoopiAiHub.Application.Utils
{
    public static class JsonSchemaToFormMapper
    {
        private const string PropertiesName = "properties";

        /// <summary>
        /// Return FormFieldDto list from json string 
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        public static List<FormFieldDto> MapToFormFields(string json)
        {
            var schema = JsonNode.Parse(json);
            var properties = schema?[PropertiesName]?.AsObject();
            var required = schema?["required"]?.AsArray()?.Select(x => x!.ToString()).ToList() ?? new();

            var result = new List<FormFieldDto>();
            if (properties != null)
            {
                foreach (var prop in properties)
                    result.Add(MapField(prop.Key, prop.Value, required));
            }

            return result;
        }


        /// <summary>
        /// Map FormFieldDto from json node
        /// </summary>
        /// <param name="name"></param>
        /// <param name="node"></param>
        /// <param name="required"></param>
        /// <returns></returns>
        private static FormFieldDto MapField(string name, JsonNode? node, List<string> required)
        {
            var type = node?["type"]?.ToString() ?? "object";
            var max = ((int?)node?["maxLength"]);
            var min = ((int?)node?["minLength"]);
            var description = ((string?)node?["description"]) ?? "";
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

            if (node?[PropertiesName] != null)
            {
                var subRequired = node["required"]?.AsArray()?.Select(x => x!.ToString()).ToList() ?? new();
                foreach (var sub in node![PropertiesName]!.AsObject())
                    field.Children!.Add(MapField(sub.Key, sub.Value, subRequired));
            }

            if (type == "array" && node?["items"]?[PropertiesName] != null)
            {
                var subRequired = node["items"]?["required"]?.AsArray()?.Select(x => x!.ToString()).ToList() ?? new();
                foreach (var sub in node["items"]![PropertiesName]!.AsObject())
                    field.Children!.Add(MapField(sub.Key, sub.Value, subRequired));
            }

            return field;
        }
    }
}
