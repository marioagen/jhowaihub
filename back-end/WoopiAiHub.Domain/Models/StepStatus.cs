using System.ComponentModel.DataAnnotations.Schema;

namespace WoopiAiHub.Domain.Models
{
    public class StepStatus : BaseEntity
    {
        [Column("Name", TypeName = "varchar(50)")]
        public string Name { get; private set; }

        public StepStatus(string name, int id, DateTime created) : base(id, created)
        {
            Name = name;
        }

        private StepStatus(int id, DateTime created) : base(id, created) { }
    }
}
