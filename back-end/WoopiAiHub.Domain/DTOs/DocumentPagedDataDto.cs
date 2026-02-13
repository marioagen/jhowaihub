using WoopiAiHub.Domain.Enum;

namespace WoopiAiHub.Domain.DTOs
{
    public class DocumentPagedDataDto
    {
        public int Page { get; set; }
        public int PageSize { get; set; }
        public string? Search { get; set; }
        public String? Login { get; set; } = null;
        public bool IsAllUsers { get; set; } = true;
        public bool IsAscending { get; set; }
        public ColTypeDocument ColType { get; set; }
        public List<int> WorkflowIds { get; set; } = new List<int>();
        public int? StatusId { get; set; }
    }
}
