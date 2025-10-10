using System.ComponentModel.DataAnnotations.Schema;

namespace WoopiAiHub.Domain.Models
{
    public class Step : BaseEntity
    {
        [Column("WorkflowId", TypeName = "int")]
        public int WorkflowId { get; private set; }

        [Column("Name", TypeName = "varchar(255)")]
        public string Name { get; private set; } = string.Empty;

        [Column("Order", TypeName = "int")]
        public int Order { get; private set; }

        [Column("ProfileId", TypeName = "int")]
        public int ProfileId { get; private set; }

        [Column("StatusId", TypeName = "int")]
        public int StatusId { get; private set; }

        public virtual Workflow? Workflow { get; set; }
        public virtual Profile? Profile { get; set; }
        public virtual Status? Status { get; set; }
        public virtual ICollection<Card> Cards { get; set; } = [];
        public virtual ICollection<StepProfilePermission> StepProfilePermissions { get; set; }

        public virtual ICollection<StepTool> StepTools { get; set; } = new List<StepTool>();

        public Step(int id, DateTime created, int workflowId, string name, int order, int profileId, int statusId)
            : base(id, created)
        {
            WorkflowId = workflowId;
            Name = name;
            Order = order;
            ProfileId = profileId;
            StatusId = statusId;
            Cards = new List<Card>();
        }

        private Step(int id, DateTime created) : base(id, created) { }

        public void Update(string name, int order, int profileId, int statusId)
        {
            Name = name;
            Order = order;
            ProfileId = profileId;
            StatusId = statusId;
        }

        public void AddCard(Card card)
        {
            ArgumentNullException.ThrowIfNull(card);

            if (Cards.Any(c => c.Id == card.Id))
                return;
            Cards.Add(card);
        }

        public void AddStepTool(StepTool stepTool)
        {
            StepTools.Add(stepTool);
        }

        public void RemoveStepTool(StepTool stepTool)
        {
            ArgumentNullException.ThrowIfNull(stepTool);
            StepTools.Remove(stepTool);
        }
    }
}
