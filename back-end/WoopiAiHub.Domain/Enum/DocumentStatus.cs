namespace WoopiAiHub.Domain.Enum
{
    public enum DocumentStatus
    {
        NotAnalyzed = 0,
        ReadyForAnalysis = 1,
        OCR = 2,
        Embeddings = 3,
        Analyzed = 4,
        Failure = 5
    }
}
