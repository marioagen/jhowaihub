using System.ComponentModel.DataAnnotations.Schema;

namespace WoopiAiHub.Domain.Models
{
    public class ModelEmbedding: BaseEntity
    {
        [Column("Name", TypeName = "varchar(100)")]
        public string Name { get; private set; }

        public virtual ICollection<UsageDaily>? UsageDaily { get; set; }
        public virtual ICollection<UsageMonth>? UsageMonth { get; set; }
        public virtual ICollection<UsageLog>? UsageLog { get; set; }

        public ModelEmbedding(int id,
                         DateTime created,
                         string name)
            : base(id, created)
        {
            Name = name;
        }
    }
}
