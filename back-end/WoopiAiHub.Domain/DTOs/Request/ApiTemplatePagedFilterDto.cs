namespace WoopiAiHub.Domain.DTOs.Request
{
    public record class ApiTemplatePagedFilterDto 
    {
        public int Page { get; set; }
        public int PageSize { get; set; }
        public string? OrderBy { get; set; } = null;
        public string? Input { get; set; } = null;
        public string? Method { get; set; } = null;
        public bool EnableAccessFromMcp { get; set; } = false;
        public int? PromptId { get; set; } = null;
    }
}