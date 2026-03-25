namespace WoopiAiHub.Domain.DTOs.Request
{
    /// <summary>
    /// Describes an outbound HTTP call to a third-party API.
    /// <see cref="Url"/> must be an absolute URI. Optional <see cref="Query"/> entries are merged into the URL via the handler.
    /// </summary>
    public class ThirdPartyApiRequestDto
    {
        public string Url { get; set; } = string.Empty;
        public string Method { get; set; } = string.Empty;
        public Dictionary<string, string>? Query { get; set; }
        public Dictionary<string, string>? Headers { get; set; }
        public string? Body { get; set; }
        public string? TemplateName { get; set; }
        public string? Tenant { get; set; }
        public string? Email { get; set; }
        public int? ExecutionId { get; set; }
    }
}
