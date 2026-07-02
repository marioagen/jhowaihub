using WoopiAiHub.Domain.Utils.AnalyzeResultAzure;

namespace WoopiAiHub.Domain.Interfaces.Utils;

public interface INativePdfTextExtractor
{
    AnalyzeResultCustomDto Extract(byte[] pdfBytes);
}
