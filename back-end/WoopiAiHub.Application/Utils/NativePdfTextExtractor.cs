using UglyToad.PdfPig;
using WoopiAiHub.Domain.Interfaces.Utils;
using WoopiAiHub.Domain.Utils.AnalyzeResultAzure;

namespace WoopiAiHub.Application.Utils;

public class NativePdfTextExtractor : INativePdfTextExtractor
{
    public AnalyzeResultCustomDto Extract(byte[] pdfBytes)
    {
        var analyzeResult = new AnalyzeResultCustomDto();
        var pages = new List<CustomDocumentPage>();

        using var document = PdfDocument.Open(pdfBytes);
        foreach (var page in document.GetPages())
        {
            var lines = page.Text
                .Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Where(line => !string.IsNullOrWhiteSpace(line))
                .Select(line => new CustomDocumentLine { Content = line })
                .ToList();

            pages.Add(new CustomDocumentPage
            {
                PageNumber = page.Number,
                Lines = lines
            });
        }

        analyzeResult.Pages = pages;
        return analyzeResult;
    }
}
