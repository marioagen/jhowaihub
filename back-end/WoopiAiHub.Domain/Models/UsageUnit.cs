using System.ComponentModel.DataAnnotations.Schema;

namespace WoopiAiHub.Domain.Models
{
    public class UsageUnit: BaseEntity
    {
        [Column("Name", TypeName = "varchar(100)")]
        public string Name { get; private set; }
        [Column("UsageTypeId", TypeName = "int")]
        public int UsageTypeId { get; private set; }

        public virtual UsageType? UsageType { get; set; }

        public UsageUnit(int id,
                         DateTime created,
                         string name,
                         int usageTypeId)
            : base(id, created)
        {
            Name = name;
            UsageTypeId = usageTypeId;
        }
    }
}
