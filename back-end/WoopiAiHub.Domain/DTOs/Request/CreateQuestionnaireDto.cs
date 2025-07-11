namespace WoopiAiHub.Domain.DTOs.Request
{
    public class CreateQuestionnaireDto
    {
        public string Title { get; set; } = string.Empty;
        public int TypeDocId { get; set; }
        public List<int> QuestionsId { get; set; }
    }
}
