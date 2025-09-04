using System.ComponentModel.DataAnnotations.Schema;

namespace WoopiAiHub.Domain.Models
{
    public class Profile : BaseEntity
    {
        [Column("Name", TypeName = "varchar(100)")]
        public string Name { get; private set; } = string.Empty;

        public virtual ICollection<User> Users { get; set; }
        public virtual ICollection<Permission> Permissions { get; set; }
        public virtual ICollection<Step> Steps { get; set; }

        public Profile(string name,
                       int id,
                       DateTime created) : base(id, created)
        {
            this.Name = name;
        }

        public void AddPermission(Permission permission)
        {
            if (permission == null)
                throw new ArgumentNullException(nameof(permission));

            if (this.Permissions.Any(t => t.Id == permission.Id))
                return;

            Permissions.Add(permission);
        }

        public void Update(string name)
        {
            Name = name;
        }

        private Profile(int id, DateTime created) : base(id, created) { }
    }
}

