namespace WoopiAiHub.Domain.Utils.AnalyzeResultAzure
{
    public class CustomDocumentPage
    {
        public int PageNumber { get; set; }
        public IEnumerable<CustomDocumentLine> Lines { get; set; } = [];
    }
}
