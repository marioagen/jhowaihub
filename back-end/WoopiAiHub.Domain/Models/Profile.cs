using System.ComponentModel.DataAnnotations.Schema;

namespace WoopiAiHub.Domain.Models
{
    public class Profile : BaseEntity
    {
        [Column("Name", TypeName = "varchar(100)")]
        public string Name { get; private set; } = string.Empty;

        public virtual ICollection<User> Users { get; set; }
        public virtual ICollection<Permission> Permissions { get; set; }

        public Profile(string name,
                       int id,
                       DateTime created) : base(id, created)
        {
            this.Name = name;
        }

        private Profile(int id, DateTime created) : base(id, created) { }
    }
}

