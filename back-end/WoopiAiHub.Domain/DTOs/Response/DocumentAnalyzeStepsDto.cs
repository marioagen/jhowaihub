namespace WoopiAiHub.Domain.DTOs.Response
{
    public class DocumentAnalyzeStepsDto
    {
        public string DocumentId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string ReferenceFile { get; set; } = string.Empty;
        public string LastProcessedStepId { get; set; } = string.Empty;
        public List<DocumentStepDto> Steps { get; set; } = new();
    }
}
