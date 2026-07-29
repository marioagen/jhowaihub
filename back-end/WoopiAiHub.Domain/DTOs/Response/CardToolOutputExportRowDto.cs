namespace WoopiAiHub.Domain.DTOs.Response
{
    /// <summary>
    /// Represents a single flattened row used for CSV export of AI tool outputs per card.
    /// Ordered by Step.Order and StepTool.Order so the export reflects the processing sequence.
    /// </summary>
    public record CardToolOutputExportRowDto
    {
        public int CardId { get; set; }
        public string DocumentName { get; set; } = string.Empty;
        public string StepName { get; set; } = string.Empty;
        public string ToolName { get; set; } = string.Empty;
        public DateTime ExecutionDate { get; set; }
        public string Output { get; set; } = string.Empty;
    }
}
