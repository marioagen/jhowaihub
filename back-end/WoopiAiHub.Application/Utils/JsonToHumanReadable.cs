using Humanizer;
using System.Text;
using System.Text.Json;

namespace WoopiAiHub.Application.Utils
{
    public static class JsonToHumanReadableExtension
    {
        public static string JsonToHumanReadable(this string content)
        {
            return FormatJsonToSimpleText(content);
        }

        /// <summary>
        /// Convert JSON to human readable text
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        private static string FormatJsonToSimpleText(string content)
        {
            try
            {
                var data = JsonSerializer.Deserialize<JsonElement>(content);
                var sb = new StringBuilder();
                FormatSimple(data, sb);
                return sb.ToString();
            }
            catch
            {
                return content;
            }
        }

        /// <summary>
        /// Format an json element
        /// </summary>
        /// <param name="element"></param>
        /// <param name="sb"></param>
        private static void FormatSimple(JsonElement element, StringBuilder sb)
        {
            foreach (var property in element.EnumerateObject())
            {
                var label = FormatLabel(property.Name);

                if (property.Value.ValueKind == JsonValueKind.Array)
                {
                    sb.AppendLine($"{label}:");
                    foreach (var item in property.Value.EnumerateArray())
                    {
                        if (item.ValueKind == JsonValueKind.Object)
                            FormatSimple(item, sb);
                    }
                }
                else if (property.Value.ValueKind == JsonValueKind.Object)
                {
                    sb.AppendLine($"{label}:");
                    FormatSimple(property.Value, sb);
                }
                else
                {
                    var value = property.Value.ValueKind == JsonValueKind.String && string.IsNullOrEmpty(property.Value.GetString())
                        ? "(não informado)"
                        : property.Value.ToString();

                    sb.AppendLine($"{label}: {value}");
                }
            }
        }

        /// <summary>
        /// Format property name as label
        /// </summary>
        /// <param name="label"></param>
        /// <returns></returns>
        private static string FormatLabel(string propertyName)
        {
            return propertyName.ApplyCase(LetterCasing.Title);
        }
    }
}
