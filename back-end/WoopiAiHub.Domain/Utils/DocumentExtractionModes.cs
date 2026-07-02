namespace WoopiAiHub.Domain.Utils;

public static class DocumentExtractionModes
{
    public const string Auto = "Auto";
    public const string Native = "Native";
    public const string Multimodal = "Multimodal";
    public const string ForceOcr = "ForceOcr";

    public static bool IsValid(string? mode)
    {
        if (string.IsNullOrWhiteSpace(mode))
            return false;

        return mode.Equals(Auto, StringComparison.OrdinalIgnoreCase)
               || mode.Equals(Native, StringComparison.OrdinalIgnoreCase)
               || mode.Equals(Multimodal, StringComparison.OrdinalIgnoreCase)
               || mode.Equals(ForceOcr, StringComparison.OrdinalIgnoreCase);
    }

    public static bool RequiresNativeProcessing(string mode) =>
        mode.Equals(Native, StringComparison.OrdinalIgnoreCase);
}
