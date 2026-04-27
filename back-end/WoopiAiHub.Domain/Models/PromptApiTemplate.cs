namespace WoopiAiHub.Domain.Models
{
    public class PromptApiTemplate : BaseEntity
    {
        public int PromptId { get; private set; }
        public virtual Prompt? Prompt { get; set; }
        public int ApiTemplateId { get; private set; }
        public virtual ApiTemplate? ApiTemplate { get; set; }

        public PromptApiTemplate(int id, int apiTemplateId, int promptId, DateTime created) : base(id, created)
        {
            ApiTemplateId = apiTemplateId;
            PromptId = promptId;
        }
        
        private PromptApiTemplate(int id, DateTime created) : base(id, created) { }
    }
}