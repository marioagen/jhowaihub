namespace WoopiAiHub.Domain.DTOs.Response
{
    public record OcrTextResponseDto
    {
        public string Content { get; set; } = string.Empty;
        public bool HasOcr { get; set; }
        public string ReferenceFile { get; set; } = string.Empty;
    }
}
