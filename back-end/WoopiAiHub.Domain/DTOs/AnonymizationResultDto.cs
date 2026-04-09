namespace WoopiAiHub.Domain.DTOs
{
    public record AnonymizationResultDto
    {
        public string DocumentUrl { get; set; } = string.Empty;
        public int WoopiAiDocumentId { get; set; }
        public string WoopiAiEmail { get; set; } = string.Empty;
    }
}
