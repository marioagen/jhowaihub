namespace WoopiAiHub.Domain.Models
{
    public class PromptApiTemplate : BaseEntity
    {
        public PromptApiTemplate(int id, int apiTemplateId, int promptId, DateTime created) : base(id, created) { }
        private PromptApiTemplate(int id, DateTime created) : base(id, created) { }
        public int PromptId { get; set; }
        public Prompt? Prompt { get; set; }
        public int ApiTemplateId { get; set; }
        public ApiTemplate? ApiTemplate { get; set; }
    }
}