using DocAnalyzer.Domain.Models;

namespace DocAnalyzer.Domain.DTOs.Response
{
    public class DocumentPagedResultDto
    {
        public IEnumerable<Document> Content { get; set; }
        public int CurrentPage { get; set; }
        public int PageCount { get; set; }
        public int RowCount { get; set; }

    }
}
