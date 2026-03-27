namespace WoopiAiHub.Domain.DTOs.Refit
{
    public record AnonymizationDocumentRequestDto
    {
        public string Name { get; init; } = string.Empty;
        public string Upload { get; init; } = string.Empty;
    }
}
