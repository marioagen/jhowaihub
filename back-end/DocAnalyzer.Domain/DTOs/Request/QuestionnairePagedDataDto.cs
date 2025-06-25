using DocAnalyzer.Domain.Enum;

namespace DocAnalyzer.Domain.DTOs.Request
{
    public class QuestionnairePagedDataDto
    {
        public int Page { get; set; }
        public int PageSize { get; set; }
        public string? Search { get; set; }
        public bool IsAscending { get; set; }
        public ColTypeQuestionnaire ColType { get; set; }
    }
}
