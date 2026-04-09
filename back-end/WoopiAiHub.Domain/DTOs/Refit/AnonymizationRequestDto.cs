namespace WoopiAiHub.Domain.DTOs.Refit
{
    public record AnonymizationRequestDto
    {
        public AnonymizationDocumentRequestDto Document { get; init; } = new();
        public int UserId { get; init; }
        public string UriResponse { get; init; } = string.Empty;
        public int? AnonymizationType { get; set; }
        public string? WoopiAiPromptId { get; init; }
        public int? WoopiAiDocumentId { get; init; }
        public string? WoopiAiEmail { get; init; }
    }
}
