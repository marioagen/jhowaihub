namespace WoopiAiHub.Domain.Utils.AnalyzeResultAzure
{
    public class AnalyzeResultCustomDto
    {
        public IEnumerable<CustomDocumentPage> Pages { get; set; } = [];
        public IEnumerable<CustomDocumentTable> Tables { get; set; } = [];
    }
}
