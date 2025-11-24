using System.ComponentModel.DataAnnotations.Schema;

namespace WoopiAiHub.Domain.Models
{
    public class UsageType: BaseEntity
    {
        [Column("Name", TypeName = "varchar(100)")]
        public string Name { get; private set; }

        public virtual ICollection<UsageUnit>? Units { get; set; }
        public virtual ICollection<UsageDaily>? UsageDaily { get; set; }
        public virtual ICollection<UsageMonth>? UsageMonth { get; set; }
        public virtual ICollection<UsageLog>? UsageLog { get; set; }

        public UsageType(int id,
                         DateTime created,
                         string name)
            : base(id, created)
        {
            Name = name;
        }
    }
}
