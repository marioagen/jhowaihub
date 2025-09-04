namespace WoopiAiHub.Domain.Enum
{
    public enum DocumentStatus
    {
        NotAnalyzed = 0,
        Analyzed = 1,
        OCR = 2,
        Embeddings = 3,
        ReadyForAnalysis = 4,
        Failure = 5
    }
}
