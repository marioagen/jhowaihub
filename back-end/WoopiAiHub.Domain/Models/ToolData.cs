using System.ComponentModel.DataAnnotations.Schema;

namespace WoopiAiHub.Domain.Models
{
    public class ToolData : BaseEntity
    {
        [Column("Name", TypeName = "varchar(50)")]
        public string Name { get; private set; } = string.Empty;
        [Column("IsActive", TypeName = "bit")]
        public bool IsActive { get; private set; }

        public virtual ICollection<Tool>? InputTools { get; set; }
        public virtual ICollection<Tool>? OutputTools { get; set; }

        public ToolData(int id, DateTime created, string name, bool isActive) : base(id, created)
        {
            Name = name;
            IsActive = isActive;
        }

        public ToolData(int id, DateTime created) : base(id, created) { }
    }
}
