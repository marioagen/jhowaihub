using System.ComponentModel.DataAnnotations.Schema;
using System.Security.Cryptography;
using WoopiAiHub.Domain.Utils;

namespace WoopiAiHub.Domain.Models
{
    public class ToolType : BaseEntity
    {
        [Column("Name", TypeName = "varchar(50)")]
        public string Name { get; private set; } = string.Empty;
        [Column("Description", TypeName = "varchar(100)")]
        public string Description { get; private set; } = string.Empty;
        [Column("IsActive", TypeName = "bit")]
        public bool IsActive { get; private set; }

        public virtual ICollection<Tool>? Tools { get; set; }

        public ToolType(int id, DateTime created, string name, string description, bool isActive) : base(id, created)
        {
            Name = name;
            Description = description;
            IsActive = isActive;
        }

        public ToolType(int id, DateTime created) : base(id, created) { }

        public bool IsN8nTool()
            => Name?.Contains(ConnectorNames.N8N, StringComparison.OrdinalIgnoreCase) == true;
    }
}
