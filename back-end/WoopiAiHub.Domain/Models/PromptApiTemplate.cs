
namespace WoopiAiHub.Domain.Models
{
    public class PromptApiTemplate : BaseEntity
    {
        public PromptApiTemplate(int id, DateTime created) : base(id, created) { }
        public int PromptId { get; set; }
        public Prompt? Prompt { get; set; }
        public int ApiTemplateId { get; set; }
        public ApiTemplate? ApiTemplate { get; set; }
    }
}