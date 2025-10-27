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
                AppendFormattedValue(property.Value, label, sb);
            }
        }

        /// <summary>
        /// Append property value
        /// </summary>
        /// <param name="value"></param>
        /// <param name="label"></param>
        /// <param name="sb"></param>
        private static void AppendFormattedValue(JsonElement value, string label, StringBuilder sb)
        {
            switch (value.ValueKind)
            {
                case JsonValueKind.Array:
                    AppendArray(value, label, sb);
                    break;

                case JsonValueKind.Object:
                    if (!string.IsNullOrEmpty(label))
                    {
                        sb.AppendLine($"{label}:");
                    }
                    FormatSimple(value, sb);
                    break;

                default:
                    AppendPrimitive(value, label, sb);
                    break;
            }
        }

        /// <summary>
        /// Append array property
        /// </summary>
        /// <param name="array"></param>
        /// <param name="label"></param>
        /// <param name="sb"></param>
        private static void AppendArray(JsonElement array, string label, StringBuilder sb)
        {
            sb.AppendLine($"{label}:");
            foreach (var item in array.EnumerateArray())
            {
                AppendFormattedValue(item, string.Empty, sb);
            }
        }

        /// <summary>
        /// Append primitive data
        /// </summary>
        /// <param name="value"></param>
        /// <param name="label"></param>
        /// <param name="sb"></param>
        private static void AppendPrimitive(JsonElement value, string label, StringBuilder sb)
        {
            var text = value.ValueKind == JsonValueKind.String && string.IsNullOrEmpty(value.GetString())
                ? "(não informado)"
                : value.ToString();

            if (string.IsNullOrEmpty(text)) return;

            if (string.IsNullOrEmpty(label))
                sb.AppendLine($"{text}");
            else
                sb.AppendLine($"{label}: {text}");
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
