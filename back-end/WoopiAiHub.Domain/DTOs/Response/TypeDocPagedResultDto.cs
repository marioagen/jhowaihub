namespace WoopiAiHub.Domain.DTOs.Response
{
    public class TypeDocPagedResultDto
    {
        public IEnumerable<TypeDocDto> Content { get; set; } = Enumerable.Empty<TypeDocDto>();
        public int CurrentPage { get; set; }
        public int PageCount { get; set; }
        public int RowCount { get; set; }
    }
}
