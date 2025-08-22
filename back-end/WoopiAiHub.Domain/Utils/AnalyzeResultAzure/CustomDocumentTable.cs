namespace WoopiAiHub.Domain.Utils.AnalyzeResultAzure
{
    public class CustomDocumentTable
    {
        public List<BoundingRegionCustom> BoundingRegions { get; set; } = [];
        public List<CustomDocumentTableCell> Cells { get; set; } = [];
    }
}
