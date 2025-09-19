namespace WoopiAiHub.Domain.DTOs.Request
{
    public record class ToolCreateDto
    {
        public string Name { get; set; } = string.Empty;
        public int ToolTypeId { get; set; }
        public int InputDataId { get; set; }
        public int OutputDataId { get; set; }
        public bool IsEditableInput { get; set; }
    }
}
