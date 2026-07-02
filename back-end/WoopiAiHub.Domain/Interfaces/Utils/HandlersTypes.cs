namespace WoopiAiHub.Domain.Utils;
public static class HandlersTypes
{
    public const string Ocr = "OCR";
    public const string Parser = "Parser";
    public const string Embeddings = "Embeddings";
    public const string Prompt = "Prompt";
    public const string N8N = "N8N";
    public const string API = "API";
    public const string Quiz = "Quiz";

    public static bool IsTextExtractionTool(string? toolTypeName) =>
        string.Equals(toolTypeName, Ocr, StringComparison.OrdinalIgnoreCase)
        || string.Equals(toolTypeName, Parser, StringComparison.OrdinalIgnoreCase);
}
