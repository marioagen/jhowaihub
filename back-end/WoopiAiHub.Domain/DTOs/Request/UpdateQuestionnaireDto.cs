namespace WoopiAiHub.Domain.DTOs.Request
{
    public class UpdateQuestionnaireDto
    {
        public string Title { get; set; } = string.Empty;
        public int TypeDocId { get; set; }
        public List<int> QuestionsId { get; set; }
        public int Id { get; set; }
    }
}
