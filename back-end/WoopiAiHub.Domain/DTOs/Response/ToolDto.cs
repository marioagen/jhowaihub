namespace WoopiAiHub.Domain.DTOs.Response
{
    public record class ToolDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int ToolTypeId { get; set; }
        public string ToolType { get; set; } = string.Empty;
        public string InputData { get; set; } = string.Empty;
        public string OutputData { get; set; } = string.Empty;
    }
}
