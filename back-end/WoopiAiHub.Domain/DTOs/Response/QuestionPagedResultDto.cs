namespace WoopiAiHub.Domain.DTOs.Response
{
    public class QuestionPagedResultDto
    {
        public IEnumerable<QuestionDto> Content { get; set; } = Enumerable.Empty<QuestionDto>();
        public int CurrentPage { get; set; }
        public int PageCount { get; set; }
        public int RowCount { get; set; }
    }
}
