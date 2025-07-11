namespace WoopiAiHub.Domain.DTOs
{
    public class PagedResponseDto<T>
    {
        public IEnumerable<T>? Items { get; set; }
        public int TotalCount { get; set; }
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
    }
}
