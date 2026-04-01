namespace WoopiAiHub.Domain.DTOs.Request
{
    public class ApiTemplateRequestCheckAssembledRequestDto
    {
        public string Method { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public Dictionary<string, string>? Headers { get; set; }
        public string? Body { get; set; }
    }
}
