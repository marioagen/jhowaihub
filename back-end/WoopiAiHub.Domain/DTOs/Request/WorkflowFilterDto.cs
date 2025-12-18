namespace WoopiAiHub.Domain.DTOs.Request
{
    public record class WorkflowFilterDto
    {
        public string? OrderBy { get; set; } = null;
        public string? Input { get; set; } = null;
        public string? Login { get; set; } = null;
        public Boolean? IsAllUsers { get; set; } = true;
    }
}