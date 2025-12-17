using WoopiAiHub.Domain.Models;

namespace WoopiAiHub.Domain.DTOs.Response
{
    public class DocumentPagedResultDto
    {
        public IEnumerable<DocumentListItemDto> Content { get; set; }
        public int CurrentPage { get; set; }
        public int PageCount { get; set; }
        public int RowCount { get; set; }
    }
}
