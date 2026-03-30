namespace WoopiAiHub.Domain.DTOs.Request
{
    /// <summary>
    /// Represents a concrete outbound HTTP request produced for API template testing: the URL, method, optional headers, and body after applying variable substitution and merging query and header templates.
    /// </summary>
    public class ApiTemplateRequestTestsAssembledRequestDto
    {
        public string Method { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public Dictionary<string, string>? Headers { get; set; }
        public string? Body { get; set; }
    }
}
