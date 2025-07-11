using WoopiAiHub.Domain.Enum;

namespace WoopiAiHub.Domain.DTOs
{
    public class DocumentPagedDataDto
    {
        public int Page { get; set; }
        public int PageSize { get; set; }
        public string? Search { get; set; }
        public bool IsAscending { get; set; }
        public ColTypeDocument ColType { get; set; }
    }
}
