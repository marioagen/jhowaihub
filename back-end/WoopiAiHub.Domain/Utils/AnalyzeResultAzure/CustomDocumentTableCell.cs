namespace WoopiAiHub.Domain.Utils.AnalyzeResultAzure
{
    public class CustomDocumentTableCell
    {
        public int RowIndex { get; set; }
        public int ColumnIndex { get; set; }
        public string Content { get; set; } = string.Empty;
    }
}
