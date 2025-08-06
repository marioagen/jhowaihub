using System.ComponentModel.DataAnnotations.Schema;

namespace WoopiAiHub.Domain.Models
{
    public class Status : BaseEntity
    {
        [Column("Name", TypeName = "varchar(50)")]
        public string Name { get; private set; }

        public virtual ICollection<Step> Steps { get; set; }
        public virtual ICollection<Card> Cards { get; set; }

        public Status(string name, int id, DateTime created) : base(id, created)
        {
            Name = name;
        }

        private Status(int id, DateTime created) : base(id, created) { }
    }
}
