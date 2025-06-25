namespace DocAnalyzer.Domain.DTOs.Refit
{
    public class FileUploadSummaryDto
    {
        public string TotalSizeUploaded { get; set; } = string.Empty;
        public string GuidId { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
    }
}
