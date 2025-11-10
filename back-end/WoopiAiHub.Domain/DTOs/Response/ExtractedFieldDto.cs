namespace WoopiAiHub.Domain.DTOs.Response
{
    public record ExtractedFieldDto
    {
        public string Label { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
        public bool IsEdited { get; set; }
        public int OutputId { get; set; }
        public string OutputType { get; set; } = string.Empty;
    }
}
