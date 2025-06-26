using WoopiAiHub.Domain.Models;

namespace WoopiAiHub.Domain.DTOs.Response
{
    public class QuestionnaireDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public int TypeDocId { get; set; }
        public string EmailCreator { get; set; } = string.Empty;
        public TypeDoc TypeDoc { get; set; }
        public string TypeDocName { get; set; } = string.Empty;
        public virtual ICollection<Question> Questions { get; set; }
        public DateTime Created { get; set; }
    }
}
