namespace WoopiAiHub.Domain.DTOs.Request
{
    public record CreateDocumentAnalysisRejectionDto(
        string Justification,
        int CardId,
        int StepId
    );
}
