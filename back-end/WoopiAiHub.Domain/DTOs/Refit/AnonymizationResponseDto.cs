namespace WoopiAiHub.Domain.DTOs.Refit
{
    public record AnonymizationResponseDto
    {
        public string StepAnonymization { get; init; } = string.Empty;
        public AnonymizationDocumentResponseDto Document { get; init; } = new();
    }
}
