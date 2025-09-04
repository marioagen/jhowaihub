using Azure.AI.FormRecognizer.DocumentAnalysis;

namespace WoopiAiHub.Domain.Interfaces.Services
{
    public interface IOcrAzure
    {
        Task<AnalyzeResult> ProcessResult(Stream stream,
                                          string tenantName);
    }
}
