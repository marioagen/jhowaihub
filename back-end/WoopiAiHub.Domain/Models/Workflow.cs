using System.ComponentModel.DataAnnotations.Schema;

namespace WoopiAiHub.Domain.Models
{
    public class Workflow : BaseEntity
    {
        [Column("TeamId", TypeName = "int")]
        public int TeamId { get; private set; }

        [Column("Name", TypeName = "varchar(255)")]
        public string Name { get; private set; } = string.Empty;

        public virtual ICollection<Step> Steps { get; set; } = [];
        public virtual Team? Team { get; set; }

        public Workflow(int id, DateTime created, int teamId, string name)
            : base(id, created)
        {
            TeamId = teamId;
            Name = name;
            Steps = new List<Step>();
        }

        /// <summary>
        /// Use to EF context
        /// </summary>
        private Workflow(int id, DateTime created) : base(id, created) { }

        public void AddStep(Step step)
        {
            ArgumentNullException.ThrowIfNull(step);

            if (Steps.Any(s => s.Id != 0 && s.Id == step.Id))
                return;
            Steps.Add(step);
        }

        public void AddSteps(ICollection<Step> steps)
        {
            foreach (var step in steps)
            {
                AddStep(step);
            }
        }      
        
        public void Update(string name)
        {
            Name = name;
        }   
    }
}
