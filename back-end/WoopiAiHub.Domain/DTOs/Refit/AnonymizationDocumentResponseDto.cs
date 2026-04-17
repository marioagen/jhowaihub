namespace WoopiAiHub.Domain.DTOs.Refit
{
    public record AnonymizationDocumentResponseDto
    {
        public string? Account { get; init; }
        public string Name { get; init; } = string.Empty;
        public string Upload { get; init; } = string.Empty;
        public string Download { get; init; } = string.Empty;
    }
}
