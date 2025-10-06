namespace WoopiAiHub.Domain.DTOs.Response
{
    public record class ToolDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int ToolTypeId { get; set; }
        public string ToolType { get; set; } = string.Empty;
        public int InputDataId { get; set; }
        public string InputData { get; set; } = string.Empty;
        public int OutputDataId { get; set; }
        public string OutputData { get; set; } = string.Empty;
        public bool IsEditableInput { get; set; }
        public string? ConnectorUrl { get; set; }
    }
}
