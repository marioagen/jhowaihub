namespace WoopiAiHub.Application.ApiTemplateRequestTests
{
    /// <summary>
    /// Maps a single JSON object in query-template and header-template arrays (key/value pairs for deserialization).
    /// </summary>
    /// <param name="Key">Header or query parameter name.</param>
    /// <param name="Value">Header or query parameter value.</param>
    public sealed record ApiTemplateRequestTestsJsonKeyValue(string Key, string Value);
}
