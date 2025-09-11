namespace WoopiAiHub.Domain.DTOs.Response
{
    public record class ToolDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public ToolTypeDto ToolType { get; set; }
        public ToolDataDto InputData { get; set; }
        public ToolDataDto OutputData { get; set; }
    }
}
