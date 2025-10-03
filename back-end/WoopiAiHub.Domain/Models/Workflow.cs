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
        public virtual ICollection<Team> Teams { get; set; } = [];
        public virtual Team? Team { get; set; }

        public Workflow(int id, DateTime created, int teamId, string name)
            : base(id, created)
        {
            TeamId = teamId;
            Name = name;
            Steps = new List<Step>();
            Teams = new List<Team>();
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
        public void AddTeam(Team team)
        {
            ArgumentNullException.ThrowIfNull(team);

            if (Teams.Any(s => s.Id != 0 && s.Id == team.Id))
                return;
            Teams.Add(team);
        }

        public void AddSteps(ICollection<Step> steps)
        {
            foreach (var step in steps)
            {
                AddStep(step);
            }
        }

        public void AddTeam(ICollection<Team> teams)
        {
            foreach (var team in teams)
            {
                AddTeam(team);
            }
        }
    }
}
