using System.Text.Json;
using System.Text.Json.Serialization;

namespace WoopiAiHub.Api.Converters;

/// <summary>
/// Ensures every DateTime leaving the API carries a UTC 'Z' suffix.
/// EF Core returns DateTime values from SQL Server with Kind=Unspecified,
/// which System.Text.Json serializes without any timezone indicator.
/// Without 'Z', date-fns parseISO treats the string as browser local time,
/// causing dates to appear offset by the server/browser timezone difference.
/// </summary>
public sealed class UtcDateTimeConverter : JsonConverter<DateTime>
{
    public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        => DateTime.SpecifyKind(reader.GetDateTime(), DateTimeKind.Utc);

    public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
        => writer.WriteStringValue(DateTime.SpecifyKind(value, DateTimeKind.Utc));
}

/// <summary>
/// Nullable counterpart of <see cref="UtcDateTimeConverter"/>.
/// </summary>
public sealed class UtcNullableDateTimeConverter : JsonConverter<DateTime?>
{
    public override DateTime? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null) return null;
        return DateTime.SpecifyKind(reader.GetDateTime(), DateTimeKind.Utc);
    }

    public override void Write(Utf8JsonWriter writer, DateTime? value, JsonSerializerOptions options)
    {
        if (value is null) { writer.WriteNullValue(); return; }
        writer.WriteStringValue(DateTime.SpecifyKind(value.Value, DateTimeKind.Utc));
    }
}
