namespace WoopiAiHub.Domain.DTOs.Request
{
    public record CreateDocumentAnalysisRejectionRangeDto(
        string Justification,
        int StepId,
        List<int> CardIds
    );
}
